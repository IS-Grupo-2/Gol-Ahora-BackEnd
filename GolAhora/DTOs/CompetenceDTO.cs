using GolAhora.Models;
using static GolAhora.DTOs.CompetenceDTO;

namespace GolAhora.DTOs
{
    public class CompetenceDTO
    {
        public class CompetenceDto
        {
            public int IdCompetence { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActive { get; set; }
            public string Regulations { get; set; } = string.Empty;
            public int CapacityTeams { get; set; }
        }

        
    }
    public class LeagueDto : CompetenceDto
    {
        public int IdLeague { get; set; }
    }
    public class TournamentDto : CompetenceDto
    {
        public int IdTournament { get; set; }
    }
}
