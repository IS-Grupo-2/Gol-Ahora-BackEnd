using Microsoft.AspNetCore.Identity;

namespace GolAhora.Models
{
    public class User: IdentityUser<int>
    {
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!;
        public bool isActive { get; set; }
        public DateTime registerDate { get; set; }


        public ClientProfile? clientProfile { get; set; }
        public PersonalClubProfile? personalClubProfile { get; set; }
    }

    public class ClientProfile
    {
        public int idClient { get; set; }

        public int idUser { get; set; } 
        public User user { get; set; } = null!;

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

    public class PersonalClubProfile
    {
        public int idPersonalClub { get; set; }

        public int idUser { get; set; }
        public User user { get; set; } = null!;

        public string legajo { get; set; } = null!;
        public DateTime startDate { get; set; }
        public string turno { get; set; } = null!;

        public AdminProfile? adminProfile { get; set; }
        public EmployeeProfile? employeeProfile { get; set; }
        public ProfessorProfile? professorProfile { get; set; }
    }

    public class AdminProfile
    {
        public int idAdmin { get; set; }

        public int idPersonalClub { get; set; }
        public PersonalClubProfile personalClubProfile { get; set; } = null!;

        public int accessLevel { get; set; }
        public ICollection<Reports> reports { get; set; } = new List<Reports>();
    }

    public class EmployeeProfile
    {
        public int idEmployee { get; set; }

        public int idPersonalClub { get; set; }
        public PersonalClubProfile personalClubProfile { get; set; } = null!;

        public string sector { get; set; } = null!;
    }

    public class ProfessorProfile
    {
        public int idProfessor { get; set; }

        public int idPersonalClub { get; set; }
        public PersonalClubProfile personalClubProfile { get; set; } = null!;

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
