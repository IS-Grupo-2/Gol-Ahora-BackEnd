namespace GolAhora.Models
{
    public class Competence
    {
        public int idCompetence { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool isActive { get; set; }
        public string regulations { get; set; } = null!;
        public int capacityTeams { get; set; }

        public ICollection<CompetenceTeam> teams { get; set; } = new List<CompetenceTeam>();
        public ICollection<Match> games { get; set; } = new List<Match>();
    }

    public class League: Competence
    {
        public int idLeague { get; set; }
    }

    public class Tournament: Competence
    {
        public int idTournament { get; set; }
    }

    public class Team
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

    public class CompetenceTeam
    {
        public int idCompetence { get; set; }
        public Competence competence { get; set; } = null!;

        public int idTeam { get; set; }
        public Team team { get; set; } = null!;

        public DateTime inscription { get; set; }
        public bool status { get; set; }

        public int idPayment { get; set; }
        public Payments payments { get; set; } = null!;
    }

    public class Match
    {
        public int idMatch { get; set; }
        public int idCompetence { get; set; }
        public Competence competence { get; set; } = null!;
        public int round { get; set; }
        public int idTeamA { get; set; }
        public Team teamA { get; set; } = null!;
        public int idTeamB { get; set; }
        public Team teamB { get; set; } = null!;
        public int idCourt { get; set; }
        public Court court { get; set; } = null!;
        public DateTime date { get; set; }
        public bool isPlayed { get; set; }
        public int idResults { get; set; }
        public Result result { get; set; } = null!;
    }

    public class Result
    {
        public int idResults { get; set; }
        public Match game { get; set; } = null!;
        public int scoreTeamLocal { get; set; }
        public int scoreTeamVisitor { get; set; }
        public int penaltiesTeamLocal { get; set; }
        public int penaltiesTeamVisitor { get; set; }
        public int foulsTeamLocal { get; set; }
        public int foulsTeamVisitor { get; set; }
        public string observations { get; set; } = null!;
    }
}
