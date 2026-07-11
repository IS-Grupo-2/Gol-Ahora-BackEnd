using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class TournamentService
    {

        public async Task<ApiResult<object>> GetCompetenciasApiContract()
        {
            var comps = await _context.Competences.Include(c => c.teams).ToListAsync();
            return ApiResult<object>.Ok(comps.Select(c => new
            {
                id = c.idCompetence,
                nombre = c.name,
                descripcion = c.description,
                tipo = c is Tournament ? "torneo" : "liga",
                estado = c.isActive ? "inscripcion" : "finalizado",
                maxEquipos = c.capacityTeams,
                equipos = c.teams.Select(t => t.idTeam),
                fechaInicio = c.startDate.ToString("yyyy-MM-dd"),
                fechaFin = c.endDate.ToString("yyyy-MM-dd"),
                precioInscripcion = 0
            }));
        }

        public async Task<ApiResult<object>> CreateCompetenciaApiContract(CompetenciaRequest request)
        {
            var validation = ValidateCompetenciaRequest(request, requireFutureStart: true);
            if (validation is not null) return ApiResult<object>.BadRequest(validation);

            Competence comp = request.Tipo == "torneo" ? new Tournament() : new League();
            comp.name = request.Nombre ?? "";
            comp.description = request.Descripcion ?? "";
            comp.startDate = request.FechaInicio ?? DateTime.UtcNow;
            comp.endDate = request.FechaFin ?? DateTime.UtcNow.AddDays(1);
            comp.isActive = request.Estado != "finalizado";
            comp.regulations = request.Regulations ?? "";
            comp.capacityTeams = request.MaxEquipos ?? 2;

            _context.Competences.Add(comp);
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { id = comp.idCompetence });
        }

        public async Task<ApiResult<object>> UpdateCompetenciaApiContract(int id, CompetenciaRequest request)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return ApiResult<object>.NotFound("Competencia inexistente.");

            var validation = ValidateCompetenciaRequest(request);
            if (validation is not null) return ApiResult<object>.BadRequest(validation);

            comp.name = request.Nombre ?? comp.name;
            comp.description = request.Descripcion ?? comp.description;
            comp.startDate = request.FechaInicio ?? comp.startDate;
            comp.endDate = request.FechaFin ?? comp.endDate;
            comp.capacityTeams = request.MaxEquipos ?? comp.capacityTeams;
            comp.isActive = request.Estado != "finalizado";
            comp.regulations = request.Regulations ?? comp.regulations;
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { id });
        }

        public async Task<ApiResult<object>> DeleteCompetenciaApiContract(int id)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return ApiResult<object>.NotFound("Competencia inexistente.");
            comp.isActive = false;
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { id });
        }

        public async Task<ApiResult<object>> InscribirEquipoApiContract(int competenciaId, InscripcionEquipoRequest request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var equipoId = request.EquipoId ?? request.IdEquipo ?? 0;
            var comp = await _context.Competences.Include(c => c.teams).FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null || !await _context.Teams.AnyAsync(t => t.idTeam == equipoId))
            {
                return ApiResult<object>.NotFound("Competencia o equipo inexistente.");
            }

            if (!comp.isActive) return ApiResult<object>.BadRequest("La competencia no esta abierta a inscripciones.");
            if (comp.teams.Count >= comp.capacityTeams) return ApiResult<object>.BadRequest("La competencia no tiene cupos disponibles.");
            if (await _context.Matches.AnyAsync(m => m.idCompetence == competenciaId)) return ApiResult<object>.BadRequest("No se puede inscribir equipos con fixture generado.");

            if (!await _context.CompetenceTeams.AnyAsync(ct => ct.idCompetence == competenciaId && ct.idTeam == equipoId))
            {
                var payment = new Payments
                {
                    idClient = await FirstClientId(),
                    amount = 0,
                    paymentDate = DateTime.UtcNow,
                    paymentMethod = "Inscripcion",
                    isSuccessful = true
                };
                _context.Payments.Add(payment);
                await _unitOfWork.SaveChangesAsync();

                _context.CompetenceTeams.Add(new CompetenceTeam
                {
                    idCompetence = competenciaId,
                    idTeam = equipoId,
                    inscription = DateTime.UtcNow,
                    status = true,
                    idPayment = payment.idPayment
                });
                await _unitOfWork.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return ApiResult<object>.Ok(new { id = competenciaId });
        }

        private static string? ValidateCompetenciaRequest(CompetenciaRequest request, bool requireFutureStart = false)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre)) return "El nombre es obligatorio.";
            if (string.IsNullOrWhiteSpace(request.Descripcion)) return "La descripcion es obligatoria.";
            if (request.FechaInicio is null) return "La fecha de inicio es obligatoria.";
            if (request.FechaFin is null) return "La fecha de fin es obligatoria.";
            if (request.FechaInicio >= request.FechaFin) return "La fecha de inicio debe ser anterior a la fecha de fin.";
            if (requireFutureStart && request.FechaInicio.Value.Date < DateTime.Today) return "La fecha de inicio no puede ser anterior a hoy.";

            var tipo = request.Tipo ?? "liga";
            var maxEquipos = request.MaxEquipos ?? 0;
            if (tipo == "torneo" && (maxEquipos < 2 || (maxEquipos & (maxEquipos - 1)) != 0))
            {
                return "Los torneos admiten 2, 4, 8, 16 o 32 equipos.";
            }

            if (tipo != "torneo" && (maxEquipos < 2 || maxEquipos > 20))
            {
                return "Las ligas admiten entre 2 y 20 equipos.";
            }

            return null;
        }

        public async Task<IActionResult> GetCompetencias()
        {
            var comps = await _context.Competences.Include(c => c.teams).ToListAsync();
            return Ok(comps.Select(c => new
            {
                id = c.idCompetence,
                nombre = c.name,
                descripcion = c.description,
                tipo = c is Tournament ? "torneo" : "liga",
                estado = c.isActive ? "inscripcion" : "finalizado",
                maxEquipos = c.capacityTeams,
                equipos = c.teams.Select(t => t.idTeam),
                fechaInicio = c.startDate.ToString("yyyy-MM-dd"),
                fechaFin = c.endDate.ToString("yyyy-MM-dd"),
                precioInscripcion = 0
            }));
        }

        public async Task<IActionResult> CreateCompetencia([FromBody] JsonElement body)
        {
            var validation = ValidateCompetencePayload(body, requireFutureStart: true);
            if (validation is not null) return ValidationError(validation);

            Competence comp = ReadString(body, "tipo") == "torneo" ? new Tournament() : new League();
            comp.name = ReadString(body, "nombre") ?? "";
            comp.description = ReadString(body, "descripcion") ?? "";
            comp.startDate = ReadDate(body, "fechaInicio") ?? DateTime.UtcNow;
            comp.endDate = ReadDate(body, "fechaFin") ?? DateTime.UtcNow.AddDays(1);
            comp.isActive = ReadString(body, "estado") != "finalizado";
            comp.regulations = ReadString(body, "regulations") ?? "";
            comp.capacityTeams = ReadInt(body, "maxEquipos") ?? 2;
            _context.Competences.Add(comp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = comp.idCompetence });
        }

        public async Task<IActionResult> UpdateCompetencia(int id, [FromBody] JsonElement body)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return NotFound();
            var validation = ValidateCompetencePayload(body);
            if (validation is not null) return ValidationError(validation);

            comp.name = ReadString(body, "nombre") ?? comp.name;
            comp.description = ReadString(body, "descripcion") ?? comp.description;
            comp.startDate = ReadDate(body, "fechaInicio") ?? comp.startDate;
            comp.endDate = ReadDate(body, "fechaFin") ?? comp.endDate;
            comp.capacityTeams = ReadInt(body, "maxEquipos") ?? comp.capacityTeams;
            comp.isActive = ReadString(body, "estado") != "finalizado";
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        public async Task<IActionResult> DeleteCompetencia(int id)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return NotFound();
            comp.isActive = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        public async Task<IActionResult> InscribirEquipo(int competenciaId, [FromBody] JsonElement body)
        {
            var equipoId = ReadInt(body, "equipoId") ?? ReadInt(body, "idEquipo") ?? 0;
            var comp = await _context.Competences.Include(c => c.teams).FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null || !await _context.Teams.AnyAsync(t => t.idTeam == equipoId)) return NotFound();
            if (!comp.isActive) return ValidationError("La competencia no esta abierta a inscripciones.");
            if (comp.teams.Count >= comp.capacityTeams) return ValidationError("La competencia no tiene cupos disponibles.");
            if (!await _context.CompetenceTeams.AnyAsync(ct => ct.idCompetence == competenciaId && ct.idTeam == equipoId))
            {
                var payment = new Payments { idClient = await FirstClientId(), amount = 0, paymentDate = DateTime.UtcNow, paymentMethod = "Inscripcion", isSuccessful = true };
                _context.Payments.Add(payment);
                await _unitOfWork.SaveChangesAsync();
                _context.CompetenceTeams.Add(new CompetenceTeam { idCompetence = competenciaId, idTeam = equipoId, inscription = DateTime.UtcNow, status = true, idPayment = payment.idPayment });
                await _unitOfWork.SaveChangesAsync();
            }
            return Ok(new { id = competenciaId });
        }

        public async Task<IActionResult> GetEquipos()
        {
            var teams = await _context.Teams.Include(t => t.captain).ThenInclude(c => c!.user).Include(t => t.players).ThenInclude(p => p.user).ToListAsync();
            return Ok(teams.Select(t => new
            {
                idEquipo = t.idTeam,
                nombre = t.name,
                capitan = t.captain == null ? "" : $"{t.captain.user.name} {t.captain.user.lastName}",
                integrantes = t.players.Select(p => $"{p.user.name} {p.user.lastName}"),
                creadoPor = t.captain == null ? null : new { idUsuario = t.captain.idUser, nombre = t.captain.user.name, apellido = t.captain.user.lastName, email = t.captain.user.Email },
                fechaCreacion = ""
            }));
        }

        public async Task<IActionResult> CreateEquipo([FromBody] JsonElement body)
        {
            var validation = await ValidateTeamPayload(body);
            if (validation is not null) return ValidationError(validation);

            var team = new Team { name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "", clientId = ReadNestedInt(body, "creadoPor", "idClient") ?? ReadInt(body, "clientId") };
            _context.Teams.Add(team);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = team.idTeam });
        }

        public async Task<IActionResult> UpdateEquipo(int id, [FromBody] JsonElement body)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return NotFound();
            var validation = await ValidateTeamPayload(body, id);
            if (validation is not null) return ValidationError(validation);

            team.name = ReadString(body, "nombre") ?? team.name;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = id });
        }

        public async Task<IActionResult> DeleteEquipo(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return NotFound();
            _context.Teams.Remove(team);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = id });
        }

        public async Task<IActionResult> GetFixtures()
        {
            var matches = await _context.Matches.Include(m => m.result).ToListAsync();
            return Ok(matches.GroupBy(m => m.idCompetence).Select(g => new
            {
                competenciaID = g.Key,
                rondas = g.GroupBy(m => m.round).Select(r => new
                {
                    numero = r.Key,
                    partidos = r.Select(m => new
                    {
                        idPartido = m.idMatch,
                        equipoLocalId = m.idTeamA,
                        equipoVisitanteId = m.idTeamB,
                        fecha = m.date.ToString("yyyy-MM-dd"),
                        estado = m.isPlayed ? "finalizado" : "programado",
                        definitivo = m.isPlayed,
                        resultado = m.result == null ? null : new { local = m.result.scoreTeamLocal, visitante = m.result.scoreTeamVisitor }
                    })
                })
            }));
        }

        public async Task<IActionResult> GenerarFixture(int competenciaId)
        {
            var comp = await _context.Competences
                .Include(c => c.teams)
                .FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null) return NotFound();

            var courtId = await _context.Courts.Select(c => c.idCourt).FirstOrDefaultAsync();
            if (courtId == 0) return BadRequest(new { message = "Debe existir al menos una cancha para generar fixture" });

            var teamIds = comp.teams.Select(t => t.idTeam).ToList();
            if (teamIds.Count < 2) return BadRequest(new { message = "La competencia necesita al menos dos equipos" });

            var previous = await _context.Matches.Include(m => m.result).Where(m => m.idCompetence == competenciaId).ToListAsync();
            _context.Results.RemoveRange(previous.Where(m => m.result != null).Select(m => m.result!));
            _context.Matches.RemoveRange(previous);

            var date = comp.startDate == default ? DateTime.UtcNow.Date : comp.startDate.Date;
            var round = 1;
            var nuevosPartidos = new List<Match>();
            for (var i = 0; i < teamIds.Count; i += 2)
            {
                if (i + 1 >= teamIds.Count) break;
                var match = new Match
                {
                    idCompetence = competenciaId,
                    round = round,
                    idTeamA = teamIds[i],
                    idTeamB = teamIds[i + 1],
                    idCourt = courtId,
                    date = date.AddDays(i / 2),
                    isPlayed = false
                };
                nuevosPartidos.Add(match);
                _context.Matches.Add(match);
            }

            comp.isActive = true;
            await _unitOfWork.SaveChangesAsync();
            foreach (var match in nuevosPartidos)
            {
                match.idResults = match.idMatch;
            }
            await _unitOfWork.SaveChangesAsync();
            return await GetFixtures();
        }

        public async Task<IActionResult> RegistrarResultado(int competenciaId, int partidoId, [FromBody] JsonElement body)
        {
            var match = await _context.Matches.Include(m => m.result).FirstOrDefaultAsync(m => m.idMatch == partidoId && m.idCompetence == competenciaId);
            if (match is null) return NotFound();

            var resultadoNode = body.TryGetProperty("resultado", out var nested) ? nested : body;
            var local = ReadInt(resultadoNode, "local") ?? ReadInt(resultadoNode, "golesLocal") ?? ReadInt(resultadoNode, "scoreTeamLocal") ?? 0;
            var visitante = ReadInt(resultadoNode, "visitante") ?? ReadInt(resultadoNode, "golesVisitante") ?? ReadInt(resultadoNode, "scoreTeamVisitor") ?? 0;
            if (local < 0 || visitante < 0) return ValidationError("Los goles no pueden ser negativos.");

            if (match.result is null)
            {
                match.result = new Result { idResults = match.idMatch };
                _context.Results.Add(match.result);
            }

            match.result.scoreTeamLocal = local;
            match.result.scoreTeamVisitor = visitante;
            match.isPlayed = true;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idPartido = partidoId, resultado = new { local, visitante } });
        }
    }
}



