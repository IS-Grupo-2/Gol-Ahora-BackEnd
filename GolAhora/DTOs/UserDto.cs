using Azure.Identity;
using System.Diagnostics.Contracts;

namespace GolAhora.DTOs
{
    public class UserDto
    {
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!; 
        public string userName { get; set; } = null!;
        public string password { get; set; } = null!;
        public string phoneNumber { get; set; } = null!;
        public bool isActive { get; set; }
    }

    public class ClientDto: UserDto
    {
        public int numberPartner { get; set; }
        public int idTeam { get; set; }
    }

    public class PersonalClubDto: UserDto
    {
            public string legajo { get; set; } = null!;
            public DateTime startDate { get; set; }
            public string turno { get; set; } = null!;
    }

    public class AdminDto: PersonalClubDto
    {
            public int accessLevel { get; set; }
    }

    public class EmployeeDto: PersonalClubDto
    {
        public string sector { get; set; } = null!;
    }

    public class ProfessorDto: PersonalClubDto 
    {
        public string specialty { get; set; } = null!;
    }
}
