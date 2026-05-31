using GolAhora.DTOs;

namespace GolAhora.Services
{
    public class MatchService
    {
        private readonly GolAhora.Data.AppContext _context;
        public MatchService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        //public async Task<string?> CreateMatch(MatchDTO matchDto)
        //{
        //    if (matchDto == null)
        //    {
        //        return "Datos invalidos";
        //    }
        //    if (matchDto.teamAId == matchDto.teamBId)
        //    {
        //        return "Los equipos no pueden ser el mismo.";
        //    }
        //    var match = new Match
        //    {
        //        teamAId = matchDto.teamAId,
        //        teamBId = matchDto.teamBId,
        //        leagueId = matchDto.leagueId,
        //        tournamentId = matchDto.tournamentId,
        //        date = matchDto.date
        //    };
        //    await _context.Matches.AddAsync(match);
        //    await _context.SaveChangesAsync();
        //    return "Partido creado exitosamente";
        //}
    }
}
