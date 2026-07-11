using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public partial class MatchService
    {
        public async Task<ApiResult<object>> GetFixturesApiContract()
        {
            var matches = await _context.Matches
                .Include(m => m.result)
                .OrderBy(m => m.idCompetence)
                .ThenBy(m => m.round)
                .ThenBy(m => m.date)
                .ThenBy(m => m.idMatch)
                .ToListAsync();

            return ApiResult<object>.Ok(matches.GroupBy(m => m.idCompetence).Select(g => new
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

        public async Task<ApiResult<object>> GenerarFixtureApiContract(int competenciaId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var comp = await _context.Competences
                .Include(c => c.teams)
                .FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null) return ApiResult<object>.NotFound("Competencia inexistente.");

            var courtId = await _context.Courts.Select(c => c.idCourt).FirstOrDefaultAsync();
            if (courtId == 0) return ApiResult<object>.BadRequest("Debe existir al menos una cancha para generar fixture.");

            var teamIds = comp.teams.Select(t => t.idTeam).OrderBy(id => id).ToList();
            if (teamIds.Count < 2) return ApiResult<object>.BadRequest("La competencia necesita al menos dos equipos.");

            var previous = await _context.Matches
                .Include(m => m.result)
                .Where(m => m.idCompetence == competenciaId)
                .ToListAsync();
            _context.Results.RemoveRange(previous.Where(m => m.result != null).Select(m => m.result!));
            _context.Matches.RemoveRange(previous);

            var startDate = comp.startDate == default ? DateTime.UtcNow.Date : comp.startDate.Date;
            var matches = comp is Tournament
                ? BuildTournamentFirstRound(competenciaId, teamIds, courtId, startDate)
                : BuildLeagueRoundRobin(competenciaId, teamIds, courtId, startDate);

            if (matches.Count == 0) return ApiResult<object>.BadRequest("No se pudieron generar partidos con los equipos inscriptos.");

            _context.Matches.AddRange(matches);
            comp.isActive = true;
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetFixturesApiContract();
        }

        public async Task<ApiResult<object>> RegistrarResultadoApiContract(int competenciaId, int partidoId, ResultadoPartidoRequest request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var match = await _context.Matches
                .Include(m => m.result)
                .Include(m => m.competence)
                .FirstOrDefaultAsync(m => m.idMatch == partidoId && m.idCompetence == competenciaId);
            if (match is null) return ApiResult<object>.NotFound("Partido inexistente.");

            var marcador = request.Resultado;
            var local = marcador?.Local ?? marcador?.GolesLocal ?? marcador?.ScoreTeamLocal
                ?? request.Local ?? request.GolesLocal ?? request.ScoreTeamLocal ?? 0;
            var visitante = marcador?.Visitante ?? marcador?.GolesVisitante ?? marcador?.ScoreTeamVisitor
                ?? request.Visitante ?? request.GolesVisitante ?? request.ScoreTeamVisitor ?? 0;
            if (local < 0 || visitante < 0) return ApiResult<object>.BadRequest("Los goles no pueden ser negativos.");
            if (match.competence is Tournament && local == visitante)
            {
                return ApiResult<object>.BadRequest("Un partido de eliminacion directa no puede finalizar empatado.");
            }

            if (match.result is null)
            {
                match.result = new Result { idResults = match.idMatch, idMatch = match.idMatch };
                _context.Results.Add(match.result);
            }

            match.result.scoreTeamLocal = local;
            match.result.scoreTeamVisitor = visitante;
            match.isPlayed = true;

            if (match.competence is Tournament)
            {
                var advanceError = await AdvanceTournamentIfRoundFinished(match.idCompetence, match.round);
                if (advanceError is not null) return ApiResult<object>.BadRequest(advanceError);
            }
            else
            {
                await FinishLeagueIfComplete(match.idCompetence);
            }

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return ApiResult<object>.Ok(new { idPartido = partidoId, resultado = new { local, visitante } });
        }

        private static List<Match> BuildTournamentFirstRound(int competenciaId, IReadOnlyList<int> teamIds, int courtId, DateTime startDate)
        {
            if (!IsPowerOfTwo(teamIds.Count)) return new List<Match>();

            var matches = new List<Match>();
            for (var i = 0; i < teamIds.Count; i += 2)
            {
                matches.Add(new Match
                {
                    idCompetence = competenciaId,
                    round = 1,
                    idTeamA = teamIds[i],
                    idTeamB = teamIds[i + 1],
                    idCourt = courtId,
                    date = startDate.AddDays(i / 2),
                    isPlayed = false
                });
            }

            return matches;
        }

        private static List<Match> BuildLeagueRoundRobin(int competenciaId, IReadOnlyList<int> teamIds, int courtId, DateTime startDate)
        {
            var ids = teamIds.Select<int, int?>(id => id).ToList();
            if (ids.Count % 2 != 0) ids.Add(null);

            var matches = new List<Match>();
            var rounds = ids.Count - 1;
            var half = ids.Count / 2;

            for (var round = 1; round <= rounds; round++)
            {
                for (var i = 0; i < half; i++)
                {
                    var home = ids[i];
                    var away = ids[ids.Count - 1 - i];
                    if (!home.HasValue || !away.HasValue) continue;

                    var invert = round % 2 == 0;
                    matches.Add(new Match
                    {
                        idCompetence = competenciaId,
                        round = round,
                        idTeamA = invert ? away.Value : home.Value,
                        idTeamB = invert ? home.Value : away.Value,
                        idCourt = courtId,
                        date = startDate.AddDays(round - 1),
                        isPlayed = false
                    });
                }

                var last = ids[^1];
                ids.RemoveAt(ids.Count - 1);
                ids.Insert(1, last);
            }

            return matches;
        }

        private async Task<string?> AdvanceTournamentIfRoundFinished(int competenciaId, int round)
        {
            var currentRound = await _context.Matches
                .Include(m => m.result)
                .Where(m => m.idCompetence == competenciaId && m.round == round)
                .OrderBy(m => m.idMatch)
                .ToListAsync();
            if (currentRound.Any(m => !m.isPlayed || m.result == null)) return null;

            var winners = currentRound
                .Select(m => m.result!.scoreTeamLocal > m.result.scoreTeamVisitor ? m.idTeamA : m.idTeamB)
                .ToList();
            if (winners.Count == 1)
            {
                var comp = await _context.Competences.FindAsync(competenciaId);
                if (comp is not null) comp.isActive = false;
                return null;
            }

            if (await _context.Matches.AnyAsync(m => m.idCompetence == competenciaId && m.round == round + 1)) return null;

            var courtId = await _context.Courts.Select(c => c.idCourt).FirstOrDefaultAsync();
            if (courtId == 0) return "Debe existir al menos una cancha para generar la siguiente ronda.";

            var baseDate = currentRound.Max(m => m.date).Date.AddDays(1);
            for (var i = 0; i < winners.Count; i += 2)
            {
                _context.Matches.Add(new Match
                {
                    idCompetence = competenciaId,
                    round = round + 1,
                    idTeamA = winners[i],
                    idTeamB = winners[i + 1],
                    idCourt = courtId,
                    date = baseDate.AddDays(i / 2),
                    isPlayed = false
                });
            }

            return null;
        }

        private async Task FinishLeagueIfComplete(int competenciaId)
        {
            var pending = await _context.Matches.AnyAsync(m => m.idCompetence == competenciaId && !m.isPlayed);
            if (pending) return;

            var comp = await _context.Competences.FindAsync(competenciaId);
            if (comp is not null) comp.isActive = false;
        }

        private static bool IsPowerOfTwo(int value) => value >= 2 && (value & (value - 1)) == 0;
    }
}
