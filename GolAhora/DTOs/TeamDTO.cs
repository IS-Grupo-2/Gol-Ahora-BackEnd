using GolAhora.Models;

namespace GolAhora.DTOs
{
    public class TeamDTO
    {
        public int idTeam { get; set; }
        public string name { get; set; } = null!;
        public int? clientId { get; set; }
        public ClientProfile? captain { get; set; } = null!;

        public ICollection<ClientProfile> players { get; set; } = new List<ClientProfile>();
        public ICollection<CompetenceTeam> competences { get; set; } = new List<CompetenceTeam>();
        public ICollection<Match> localMatches { get; set; } = new List<Match>();
        public ICollection<Match> visitorMatches { get; set; } = new List<Match>();
    }
}
