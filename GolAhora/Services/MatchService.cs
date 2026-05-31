using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class MatchService
    {
        private readonly GolAhora.Data.AppContext _context;
        public MatchService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        public async Task<string?> CreateMatch(MatchDTO matchDto) //int idCompetence, int idTeamA, int idTeamB, int idCourt, DateTime date
        {
            if (matchDto == null)
            {
                return "Datos invalidos";
            }
            var competence = await _context.Competences.FindAsync(matchDto.idCompetence);
            if (competence == null)
                return "La competencia no existe.";
            if (competence.startDate > matchDto.date)
                return "La competencia aún no ha comenzado.";
            if (competence.endDate < matchDto.date)
                return "La competencia ya ha terminado.";
            if (!competence.isActive)
                return "La competencia no está activa.";

            var teamA = await _context.Teams.FindAsync(matchDto.idTeamA);
            if (teamA == null)
                return "El equipo A no existe.";
            //if (competence.teams.Contains(teamA))
            //    return "El equipo local no esta inscripto en el torneo elegido";

            var teamB = await _context.Teams.FindAsync(matchDto.idTeamB);
            if (teamB == null)
                return "El equipo B no existe.";
            //if (competence.teams.Contains(teamB))
            //    return "El equipo local no esta inscripto en el torneo elegido";

            var court = await _context.Courts.FindAsync(matchDto.idCourt);
            if (court == null)
                return "La cancha no existe.";

            if (matchDto.idTeamA == matchDto.idTeamB)
            {
                return "Los equipos no pueden ser el mismo.";
            }
            var partidos = await _context.Matches.ToListAsync();
            int contador = partidos.Count() + 1;

            var resultados = await _context.Results.ToListAsync();
            int contadorResultados = resultados.Count() + 1;

            var resultado = new Result
            {
                idResults = contadorResultados,
                idMatch = contador,
                scoreTeamLocal = 0,
                scoreTeamVisitor = 0

                //        public int idResults { get; set; }
                //public int idMatch { get; set; }
                //public int scoreTeamLocal { get; set; }
                //public int scoreTeamVisitor { get; set; }

                //         public int idResults { get; set; }
                //public Match game { get; set; } = null!;
                //public int scoreTeamLocal { get; set; }
                //public int scoreTeamVisitor { get; set; }
                //public int penaltiesTeamLocal { get; set; } //NO ME INTERESA QUE ESTEN
                //public int penaltiesTeamVisitor { get; set; } //NO ME INTERESA QUE ESTEN
                //public int foulsTeamLocal { get; set; } //NO ME INTERESA QUE ESTEN
                //public int foulsTeamVisitor { get; set; } //NO ME INTERESA QUE ESTEN
                //public string observations { get; set; } = null!; //NO ME INTERESA QUE ESTEN
            };
            await _context.Results.AddAsync(resultado);
            await _context.SaveChangesAsync();


            var match = new Match
            {
                //public int idMatch { get; set; }
                //public int idCompetence { get; set; }
                //public Competence competence { get; set; } = null!; //ESTARIA DE MAS, SI YA TENGO EL ID
                //public int round { get; set; }
                //public int idTeamA { get; set; }
                //public Team teamA { get; set; } = null!; //ESTARIA DE MAS, SI YA TENGO EL ID
                //public int idTeamB { get; set; }
                //public Team teamB { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID
                //public int idCourt { get; set; }
                //public Court court { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID
                //public DateTime date { get; set; }
                //public bool isPlayed { get; set; }
                //public int idResults { get; set; }
                //public Result result { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID
                idMatch = contador,
                idCompetence = matchDto.idCompetence,
                competence = competence,
                round = matchDto.round,
                idTeamA = matchDto.idTeamA,
                teamA = teamA,
                idTeamB = matchDto.idTeamB,
                teamB = teamB,
                idCourt = matchDto.idCourt,
                court = court,
                date = matchDto.date,
                isPlayed = false,
                idResults = contadorResultados,
                result = resultado
            };

            teamA.localMatches.Add(match);
            teamB.visitorMatches.Add(match);

            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();
            return "Partido creado exitosamente";
        }

        //public async Task<string> UpdateMatch(int matchId, MatchDTO matchDto)  NO SE SI HACER UNA MODIFICACION DEL RESULTADO ACA O EN EL RESULT SERVICE, 
        //SI LO HAGO ACA, DEBERIA RECIBIR UN MATCHDTO CON LOS DATOS DEL PARTIDO Y LOS RESULTADOS ACTUALIZADOS, SI LO HAGO EN EL RESULT SERVICE, SOLO RECIBO 
        //UN RESULTDTO CON LOS DATOS DEL RESULTADO A ACTUALIZAR Y EL ID DEL PARTIDO AL QUE PERTENECE, POR AHORA LO DEJO EN EL RESULT SERVICE
        //{


        //    return "Partido modificado exitosamente";
        //}

        public async Task<(MatchDTO partido, string mensaje)> GetMatchById(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                return (new MatchDTO(), "El partido no existe");
            return (new MatchDTO
            {
                idMatch = match.idMatch,
                idCompetence = match.idCompetence,
                round = match.round,
                idTeamA = match.idTeamA,
                idTeamB = match.idTeamB,
                idCourt = match.idCourt,
                date = match.date
            }, "Partido encontrado exitosamente");
        }
        public async Task<(List<MatchDTO> matches, string mensaje)> GetAllMatches()
        {
            var matches = await _context.Matches.ToListAsync();
            if (matches == null || matches.Count == 0)
                return (new List<MatchDTO>(), "No hay partidos disponibles");
            return (matches.Select(match => new MatchDTO
            {
                idMatch = match.idMatch,
                idCompetence = match.idCompetence,
                round = match.round,
                idTeamA = match.idTeamA,
                idTeamB = match.idTeamB,
                idCourt = match.idCourt,
                date = match.date
            }).ToList(), "Partidos encontrados exitosamente");
        }
    }
}