using GolAhora.Models;

namespace GolAhora.DTOs
{
    public class MatchDTO
    {
        public int idMatch { get; set; }
        public int idCompetence { get; set; }
        //public Competence competence { get; set; } = null!;
        public int round { get; set; }
        public int idTeamA { get; set; }
        //public Team teamA { get; set; } = null!;
        public int idTeamB { get; set; }
        //public Team teamB { get; set; } = null!;
        public int idCourt { get; set; }
        //public Court court { get; set; } = null!;
        public DateTime date { get; set; }
        public bool isPlayed { get; set; }
        public int idResults { get; set; }
        //public Result result { get; set; } = null!;
    }
}
