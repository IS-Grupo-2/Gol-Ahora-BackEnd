namespace GolAhora.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!;
        public string userName { get; set; } = null!;
        public string password { get; set; } = null!;
        public string email { get; set; } = null!;
        public string phoneNumber { get; set; } = null!;
        public bool isActive { get; set; }
        public DateTime registerDate { get; set; }
    }

    public class Client: User
    {
        public int numberPartner { get; set; }
        public int? idTeam { get; set; }
        public Team? team { get; set; }

        /**
         Para la relacion 1 a muchos de cliente a reserva, 
         se agrega una lista con las reservas.
         */
        public ICollection<Reservation> reservations { get; set; } = new List<Reservation>();

        /**
         Para la relacion 1 a muchos de cliente a asistencia, 
         se agrega una lista con las asistencias.
         */
        public ICollection<Assistance> assistances { get; set; } = new List<Assistance>();

        public ICollection<Team> teamsCaptain { get; set; } = new List<Team>();
    }

    public class PersonalClub: User
    {
        public string legajo { get; set; } = null!;
        public DateTime startDate { get; set; }
        public string turno { get; set; } = null!;
    }

    public class Admin: PersonalClub
    {
        public int accessLevel { get; set; }
        public ICollection<Reports> reports { get; set; } = new List<Reports>();
    }

    public class Employee: PersonalClub
    {
        public string sector { get; set; } = null!;
    }

    public class Professor: PersonalClub
    {
        public string specialty { get; set; } = null!;

        /**
         Para la relacion 1 a muchos de profesor a certificado, 
         se agrega una lista con las certificaciones.
         */
        public ICollection<Certification> certifications { get; set; } = new List<Certification>();

        /**
         Para la relacion 1 a muchos de profesor a clase, 
         se agrega una lista con las clases que se le asignan a un profesor.
         */
        public ICollection<Class> classes { get; set; } = new List<Class>();
    }
}
