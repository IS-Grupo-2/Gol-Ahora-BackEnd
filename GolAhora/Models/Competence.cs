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
        public Client? captain { get; set; } = null!;

        public ICollection<Client> players { get; set; } = new List<Client>();
        public ICollection<CompetenceTeam> competences { get; set; } = new List<CompetenceTeam>(); //LO HARIA CON TEAMS Y AL INCRIBIR GENERAR LLAMAR A LA API DE PAGOS PARA GENERAR EL PAGO 
        //Y ASOCIARLO A LA INSCRIPCION, ENTONCES NO NECESITARIA UNA CLASE INTERMEDIA, SOLO UNA LISTA DE COMPETENCIAS EN TEAM Y UNA LISTA DE TEAMS EN COMPETENCE
        public ICollection<Match> localMatches { get; set; } = new List<Match>();
        public ICollection<Match> visitorMatches { get; set; } = new List<Match>(); //HARIA UNA SOLA LISTA DE MATCHES
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
        public Competence competence { get; set; } = null!; //ESTARIA DE MAS, SI YA TENGO EL ID O ES LA RELACION ?
        public int round { get; set; }
        public int idTeamA { get; set; }
        public Team teamA { get; set; } = null!; //ESTARIA DE MAS, SI YA TENGO EL ID O ES LA RELACION ?
        public int idTeamB { get; set; }
        public Team teamB { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID O ES LA RELACION ?
        public int idCourt { get; set; }
        public Court court { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID O ES LA RELACION ?
        public DateTime date { get; set; }
        public bool isPlayed { get; set; }
        public int idResults { get; set; }
        public Result result { get; set; } = null!;//ESTARIA DE MAS, SI YA TENGO EL ID O ES LA RELACION ?
    }

    public class Result
    {
        public int idResults { get; set; }
        public int idMatch { get; set; }
        public Match match { get; set; } = null!;
        public int scoreTeamLocal { get; set; }
        public int scoreTeamVisitor { get; set; }
        //public int penaltiesTeamLocal { get; set; } //NO ME INTERESA QUE ESTEN
        //public int penaltiesTeamVisitor { get; set; } //NO ME INTERESA QUE ESTEN
        //public int foulsTeamLocal { get; set; } //NO ME INTERESA QUE ESTEN
        //public int foulsTeamVisitor { get; set; } //NO ME INTERESA QUE ESTEN
        //public string observations { get; set; } = null!; //NO ME INTERESA QUE ESTEN
    }
}
