using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Exceptions;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Command
{
    public class ClientCommand
    {
        private readonly AppContext _appContext;

        public ClientCommand(AppContext appContext)
        {
            _appContext = appContext;
        }

        public async Task AddClient(ClientProfile client)
        {
            _appContext.ClientProfiles.Add(client);
            await _appContext.SaveChangesAsync();
        }

        public async Task addPersonalClub(PersonalClubProfile per)
        {
            _appContext.PersonalClubProfiles.Add(per);
            await _appContext.SaveChangesAsync();
        }

        public async Task addEmployee(EmployeeProfile employee)
        {
            _appContext.EmployeeProfiles.Add(employee);
            await _appContext.SaveChangesAsync();
        }

        public async Task addProfessor(ProfessorProfile professor)
        {
            _appContext.ProfessorProfiles.Add(professor);
            await _appContext.SaveChangesAsync();
        }

        public async Task updateUser(int id, UpdateUserDTO upUser)
        {
            var user = await _appContext.Users.FindAsync(id);

            if (user == null)
                throw new NotFoundException("El usuario no se encontro");

            user.name = upUser.name;
            user.lastName = upUser.lastName;
            user.DNI = upUser.DNI;
            user.UserName = upUser.userName;
            user.Email = upUser.email;
            user.PhoneNumber = upUser.phoneNumber;

            _appContext.Users.Update(user);
            await _appContext.SaveChangesAsync();
        }
    }
}
