using Azure.Identity;
using System.Diagnostics.Contracts;

namespace GolAhora.DTOs
{
    public class RegisterUserDto
    {
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!;
        public string userName { get; set; } = null!;
        public string email { get; set; } = null!;
        public string phoneNumber { get; set; } = null!;
        public string password { get; set; } = null!;
        public string role { get; set; } = null!;
    }

    public class LoginUserDto
    {
        public string userName { get; set; } = null!;
        public string password { get; set; } = null!;
    }

    public class LoginUserResponce
    {
        public string token { get; set; } = null!;
        public DateTime expiration { get; set; }

        public UserDto user { get; set; } = null!;
    }

    public class UserDto
    {
        public int idUser { get; set; }
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!; 
        public string userName { get; set; } = null!;
        public string email { get; set; } = null!;
        public string phoneNumber { get; set; } = null!;
        public bool isActive { get; set; }

        public IList<string> roles { get; set; } = new List<string>();
    }

    public class RegisterClientDto: RegisterUserDto
    {
        public int numberPartner { get; set; }
        public int? idTeam { get; set; }
    }

    public class RegisterPersonalClubDto: RegisterUserDto
    {
            public string legajo { get; set; } = null!;
            public DateTime startDate { get; set; }
            public string turno { get; set; } = null!;
    }

    public class RegisterAdminDto: RegisterPersonalClubDto
    {
            public int accessLevel { get; set; }
    }

    public class RegisterEmployeeDto: RegisterPersonalClubDto
    {
        public string sector { get; set; } = null!;
    }

    public class RegisterProfessorDto: RegisterPersonalClubDto 
    {
        public string specialty { get; set; } = null!;
    }

    public class ClientDto : UserDto
    {
        public int idClient { get; set; }
        public int numberPartner { get; set; }
        public int? idTeam { get; set; }
    }

    public class PersonalClubDto: UserDto
    {
        public int idPersonalClub { get; set; }
        public string legajo { get; set; } = null!;
        public DateTime startDate { get; set; }
        public string turno { get; set; } = null!;
    }

    public class AdminDto: PersonalClubDto
    {
        public int idAdmin { get; set; }
        public int accessLevel { get; set; }
    }

    public class EmployeeDto: PersonalClubDto
    {
        public int idEmployee { get; set; }
        public string sector { get; set; } = null!;
    }

    public class ProfessorDto: PersonalClubDto
    {
        public int idProfessor { get; set; }
        public string speciality { get; set; } = null!;
    }

    public class UpdateUserDTO 
    {
        public string name { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string DNI { get; set; } = null!;
        public string userName { get; set; } = null!;
        public string email { get; set; } = null!;
        public string phoneNumber { get; set; } = null!;
    }

    public class UpdateClientDTO : UpdateUserDTO
    {
        public int? idTeam { get; set; }
    }

    public class UpdatePersonalClubDTO : UpdateUserDTO
    {
        public string turno { get; set; } = null!;
    }

    public class UpdateAdminDTO : UpdatePersonalClubDTO
    {
        public int accessLevel { get; set; }
    }

    public class UpdateEmployeeDTO : UpdatePersonalClubDTO
    {
        public string sector { get; set; } = null!;
    }
    
    public class UpdateProfessorDTO : UpdatePersonalClubDTO
    {
        public string speciality { get; set; } = null!;
    }
}
