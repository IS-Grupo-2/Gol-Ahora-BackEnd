using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace GolAhora.Services
{
    public class TeamService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }

        public async Task<string?> CreateTeam(TeamDTO teamDto)
        {
            if (teamDto == null)
            {
                return "Datos invalidos";
            }
            if (string.IsNullOrEmpty(teamDto.name))
            {
                return "El nombre del equipo es obligatorio.";
            }
            var team = new Team
            {
                name = teamDto.name,
                clientId = teamDto.clientId,
                captain = teamDto.captain
            };
            await _context.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync();
            return "Equipo creado exitosamente";
        }

        public async Task<string?> UpdateTeam(int idTeam, TeamDTO teamDto)
        {
            if (teamDto == null)
            {
                return "Datos invalidos";
            }
            if (string.IsNullOrEmpty(teamDto.name))
            {
                return "El nombre del equipo es obligatorio.";
            }
            var team = await _context.Teams.FindAsync(idTeam);
            if (team == null)
            {
                return "Equipo no encontrado.";
            }
            team.name = teamDto.name;
            team.clientId = teamDto.clientId;
            team.captain = teamDto.captain;
            _context.Teams.Update(team);
            await _unitOfWork.SaveChangesAsync();
            return "Equipo actualizado exitosamente";
        }
        public async Task<(TeamDTO team, string message)> GetTeamById(int idTeam)
        {
            var team = await _context.Teams.FindAsync(idTeam);
            if (team == null)
            {
                return (new TeamDTO(), "No se encontraron equipos");
            }
            var teamDto = new TeamDTO
            {
                idTeam = team.idTeam,
                name = team.name,
                clientId = team.clientId,
                captain = team.captain,
                players = team.players,
                competences = team.competences,
                localMatches = team.localMatches,
                visitorMatches = team.visitorMatches
            };
            return (teamDto, "Equipo encontrado exitosamente");
        }
        public async Task<string?> DeleteTeam(int idTeam)
        {
            var team = await _context.Teams.FindAsync(idTeam);
            if (team == null)
            {
                return "Equipo no encontrado.";
            }
            _context.Teams.Remove(team);
            await _unitOfWork.SaveChangesAsync();
            return "Equipo eliminado exitosamente";
        }

        public async Task<(List<TeamDTO> teams, string message)> GetAllTeams()
        {
            var teams = await _context.Teams.ToListAsync();
            if (teams == null || teams.Count == 0)
            {
                return (new List<TeamDTO>(), "No se encontraron equipos");
            }
            return (teams.Select(team => new TeamDTO
            {
                idTeam = team.idTeam,
                name = team.name,
                clientId = team.clientId,
                captain = team.captain,
                players = team.players,
                competences = team.competences,
                localMatches = team.localMatches,
                visitorMatches = team.visitorMatches
            }).ToList(), "Equipos encontrados exitosamente");
        }

        public async Task<string> AddMember(int idTeam, int idPlayer) // Validar con la rama main
        {
            if (idTeam <= 0 || idPlayer <= 0)
            {
                return "Datos invalidos";
            }

            var team = await _context.Teams.FindAsync(idTeam);
            if (team == null) {
                return "Equipo no encontrado.";
            }
            var player = await _context.ClientProfiles.FindAsync(idPlayer);
            if (player == null) {
                return "Jugador no encontrado.";
            }
            if (player.idTeam != null) 
            {
                return "El jugador ya pertenece a un equipo";
            }
            team.players.Add(player);
            _context.Teams.Update(team);
            await _unitOfWork.SaveChangesAsync();
            return "Miembro agregado exitosamente";
        }

        public async Task<string> DelegateACaptain(int idTeam, int idPlayer)// Validar con la rama main
        {
            var player = await _context.ClientProfiles.FindAsync(idPlayer);
            if (player == null)
            {
                return "Jugador no encontrado.";
            }
            var team = await _context.Teams.FindAsync(idTeam);
            if (team == null) {
                return "Equipo no encontrado.";
            }
            if (!team.players.Contains(player))
            {
                return "El jugador no pertenece a este equipo.";
            }
            team.captain = player;
            _context.Teams.Update(team);
            await _unitOfWork.SaveChangesAsync();
            return "Capitan asignado correctamente";
        }

        public async Task<ApiResult<object>> GetEquiposApiContract()
        {
            var teams = await _context.Teams
                .Include(t => t.captain)!.ThenInclude(c => c!.user)
                .Include(t => t.players).ThenInclude(p => p.user)
                .ToListAsync();

            return ApiResult<object>.Ok(teams.Select(t => new
            {
                idEquipo = t.idTeam,
                nombre = t.name,
                capitan = t.captain == null ? "" : $"{t.captain.user.name} {t.captain.user.lastName}",
                integrantes = t.players.Select(p => $"{p.user.name} {p.user.lastName}"),
                creadoPor = t.captain == null ? null : new { idUsuario = t.captain.idUser, nombre = t.captain.user.name, apellido = t.captain.user.lastName, email = t.captain.user.Email },
                fechaCreacion = ""
            }));
        }

        public async Task<ApiResult<object>> CreateEquipoApiContract(EquipoApiRequest request)
        {
            var validation = await ValidateEquipoApiRequest(request);
            if (validation is not null) return ApiResult<object>.BadRequest(validation);

            var team = new Team
            {
                name = request.Nombre ?? request.Name ?? "",
                clientId = request.CreadoPor?.IdClient ?? request.CreadoPor?.IdCliente ?? request.ClientId
            };
            _context.Teams.Add(team);
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { idEquipo = team.idTeam });
        }

        public async Task<ApiResult<object>> UpdateEquipoApiContract(int id, EquipoApiRequest request)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return ApiResult<object>.NotFound("Equipo inexistente.");

            var validation = await ValidateEquipoApiRequest(request, id);
            if (validation is not null) return ApiResult<object>.BadRequest(validation);

            team.name = request.Nombre ?? request.Name ?? team.name;
            team.clientId = request.CreadoPor?.IdClient ?? request.CreadoPor?.IdCliente ?? request.ClientId ?? team.clientId;
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { idEquipo = id });
        }

        public async Task<ApiResult<object>> DeleteEquipoApiContract(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return ApiResult<object>.NotFound("Equipo inexistente.");
            if (await _context.CompetenceTeams.AnyAsync(ct => ct.idTeam == id))
            {
                return ApiResult<object>.BadRequest("No se puede eliminar un equipo inscripto en una competencia.");
            }

            _context.Teams.Remove(team);
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<object>.Ok(new { idEquipo = id });
        }

        private async Task<string?> ValidateEquipoApiRequest(EquipoApiRequest request, int? existingTeamId = null)
        {
            var name = request.Nombre ?? request.Name;
            if (string.IsNullOrWhiteSpace(name)) return "El nombre del equipo es obligatorio.";
            if (await _context.Teams.AnyAsync(t => t.name == name && (!existingTeamId.HasValue || t.idTeam != existingTeamId.Value)))
            {
                return "Ya existe un equipo con ese nombre.";
            }

            var clientId = request.CreadoPor?.IdClient ?? request.CreadoPor?.IdCliente ?? request.ClientId;
            if (clientId.HasValue && !await _context.ClientProfiles.AnyAsync(c => c.idClient == clientId.Value))
            {
                return "El cliente creador del equipo no existe.";
            }

            return null;
        }
    }
}



