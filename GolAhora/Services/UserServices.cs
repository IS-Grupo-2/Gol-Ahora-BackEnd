using GolAhora.DTOs;
using GolAhora.Exceptions;
using GolAhora.Models;
using GolAhora.Query;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolAhora.Services
{
    public class UserServices
    {
        private readonly UserManager<User> _userManager;
        private readonly UserQuery _userQuery;

        public UserServices(UserManager<User> userManager, UserQuery userQuery) { 
            _userManager = userManager;
            _userQuery = userQuery;
        }

        public async Task<UserDto> Me(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                throw new NotFoundException("El usuario no se encontro");

            var role = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                idUser = user.Id,
                name = user.name,
                lastName = user.lastName,
                DNI = user.DNI,
                userName = user.UserName!,
                email = user.Email!,
                phoneNumber = user.PhoneNumber!,
                isActive = user.isActive,
                roles = role
            };
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            var users = await _userQuery.GetAllUser();

            if (users == null || users.Count == 0)
                throw new NotFoundException("No se encontraron usuarios");

            var result = new List<UserDto>();


            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Client") && user.clientProfile != null)
                {
                    result.Add(new ClientDto
                    {
                        idUser = user.Id,
                        idClient = user.clientProfile.idClient,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        numberPartner = user.clientProfile.numberPartner,
                        idTeam = user.clientProfile.idTeam
                    });

                    continue;
                }

                if (roles.Contains("Admin") && user.personalClubProfile?.adminProfile != null)
                {
                    result.Add(new AdminDto
                    {
                        idUser = user.Id,
                        idPersonalClub = user.personalClubProfile.idPersonalClub,
                        idAdmin = user.personalClubProfile.adminProfile.idAdmin,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        legajo = user.personalClubProfile.legajo,
                        startDate = user.personalClubProfile.startDate,
                        turno = user.personalClubProfile.turno,
                        accessLevel = user.personalClubProfile.adminProfile.accessLevel
                    });

                    continue;
                }

                if (roles.Contains("Employee") && user.personalClubProfile?.employeeProfile != null)
                {
                    result.Add(new EmployeeDto
                    {
                        idUser = user.Id,
                        idPersonalClub = user.personalClubProfile.idPersonalClub,
                        idEmployee = user.personalClubProfile.employeeProfile.idEmployee,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        legajo = user.personalClubProfile.legajo,
                        startDate = user.personalClubProfile.startDate,
                        turno = user.personalClubProfile.turno,
                        sector = user.personalClubProfile.employeeProfile.sector
                    });
                    continue;
                }

                if (roles.Contains("Professor") && user.personalClubProfile?.professorProfile != null)
                {
                    result.Add(new ProfessorDto
                    {
                        idUser = user.Id,
                        idPersonalClub = user.personalClubProfile.idPersonalClub,
                        idProfessor = user.personalClubProfile.professorProfile.idProfessor,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        legajo = user.personalClubProfile.legajo,
                        startDate = user.personalClubProfile.startDate,
                        turno = user.personalClubProfile.turno,
                        speciality = user.personalClubProfile.professorProfile.speciality
                    });
                    continue;
                }

                result.Add(new UserDto
                {
                    idUser = user.Id,
                    name = user.name,
                    lastName = user.lastName,
                    DNI = user.DNI,
                    userName = user.UserName!,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles
                });
            }

            return result;
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                throw new NotFoundException("El usuario no se encontro");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Client") && user.clientProfile != null)
            {
                return new ClientDto
                {
                    idUser = user.Id,
                    idClient = user.clientProfile.idClient,
                    name = user.name,
                    lastName = user.lastName,
                    DNI = user.DNI,
                    userName = user.UserName!,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles,
                    numberPartner = user.clientProfile.numberPartner,
                    idTeam = user.clientProfile.idTeam
                };
            }

            if (roles.Contains("Admin") && user.personalClubProfile?.adminProfile != null)
            {
                return new AdminDto
                {
                    idUser = user.Id,
                    idPersonalClub = user.personalClubProfile.idPersonalClub,
                    idAdmin = user.personalClubProfile.adminProfile.idAdmin,
                    name = user.name,
                    lastName = user.lastName,
                    DNI = user.DNI,
                    userName = user.UserName!,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles,
                    legajo = user.personalClubProfile.legajo,
                    startDate = user.personalClubProfile.startDate,
                    turno = user.personalClubProfile.turno,
                    accessLevel = user.personalClubProfile.adminProfile.accessLevel
                };
            }

            if (roles.Contains("Employee") && user.personalClubProfile?.employeeProfile != null)
            {
                return new EmployeeDto
                {
                    idUser = user.Id,
                    idPersonalClub = user.personalClubProfile.idPersonalClub,
                    idEmployee = user.personalClubProfile.employeeProfile.idEmployee,
                    name = user.name,
                    lastName = user.lastName,
                    DNI = user.DNI,
                    userName = user.UserName!,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles,
                    legajo = user.personalClubProfile.legajo,
                    startDate = user.personalClubProfile.startDate,
                    turno = user.personalClubProfile.turno,
                    sector = user.personalClubProfile.employeeProfile.sector
                };
            }

            if (roles.Contains("Professor") && user.personalClubProfile?.professorProfile != null)
            {
                return new ProfessorDto
                {
                    idUser = user.Id,
                    idPersonalClub = user.personalClubProfile.idPersonalClub,
                    idProfessor = user.personalClubProfile.professorProfile.idProfessor,
                    name = user.name,
                    lastName = user.lastName,
                    DNI = user.DNI,
                    userName = user.UserName!,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles,
                    legajo = user.personalClubProfile.legajo,
                    startDate = user.personalClubProfile.startDate,
                    turno = user.personalClubProfile.turno,
                    speciality = user.personalClubProfile.professorProfile.speciality
                };
            }

            return new UserDto
            {
                idUser = user.Id,
                name = user.name,
                lastName = user.lastName,
                DNI = user.DNI,
                userName = user.UserName!,
                email = user.Email!,
                phoneNumber = user.PhoneNumber!,
                isActive = user.isActive,
                roles = roles
             };
        }

        public async Task<UserDto> UpdateUser(int id, UpdateUserDTO upUser)
        {
            var user = await _userManager.FindByIdAsync(id.ToString()); 

            if(user == null)
                throw new NotFoundException("El usuario no se encontro");


            user.name = upUser.name;
            user.lastName = upUser.lastName;
            user.DNI = upUser.DNI;
            user.UserName = upUser.userName;
            user.Email = upUser.email;
            user.PhoneNumber = upUser.phoneNumber;

            await _userManager.UpdateAsync(user);

            return new UserDto
            {
                idUser = user.Id,
                name = user.name,
                lastName = user.lastName,
                DNI = user.DNI,
                userName = user.UserName!,
                email = user.Email!,
                phoneNumber = user.PhoneNumber!,
                isActive = user.isActive,
                roles = await _userManager.GetRolesAsync(user)
            };
        }

        public async Task<string> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                throw new NotFoundException("El usuario no se encontro");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Client"))
            {
                var clientProfile = await _userQuery.GetClientProfile(id);
                if (clientProfile == null)
                    throw new NotFoundException("El perfil del cliente no se encontro");

                var hasReservations = await _userQuery.ClientHasReservations(clientProfile.idClient);

                if (hasReservations)
                    return ("El cliente no se puede dar de baja por que tiene reservas asociadas");
            }

            user.isActive = false;

            var result = await _userManager.UpdateAsync(user);
             if(!result.Succeeded)
                return ("No se pudo eliminar el usuario");

             return ("El usuario se elimino correctamente");
        }

        public async Task<List<ClientDto>> GetAllClients()
        {
            var users = await _userQuery.GetAllUser();

            var clients = new List<ClientDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Client") && user.clientProfile != null)
                {
                    clients.Add(new ClientDto
                    {
                        idUser = user.Id,
                        idClient = user.clientProfile.idClient,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        numberPartner = user.clientProfile.numberPartner,
                        idTeam = user.clientProfile.idTeam
                    });
                }
            }

            if (clients.Count == 0)
                throw new NotFoundException("No se encontraron profesores");

            return clients;
        }

        public async Task<List<EmployeeDto>> GetAllEmployees()
        {
            var users = await _userQuery.GetAllUser();

            var employee = new List<EmployeeDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Employee") && user.personalClubProfile?.employeeProfile != null)
                {
                    employee.Add(new EmployeeDto
                    {
                        idUser = user.Id,
                        idPersonalClub = user.personalClubProfile.idPersonalClub,
                        idEmployee = user.personalClubProfile.employeeProfile.idEmployee,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        legajo = user.personalClubProfile.legajo,
                        startDate = user.personalClubProfile.startDate,
                        turno = user.personalClubProfile.turno,
                        sector = user.personalClubProfile.employeeProfile.sector
                    });
                }
            }

            if (employee.Count == 0)
                throw new NotFoundException("No se encontraron profesores");

            return employee;
        }

        public async Task<List<ProfessorDto>> GetAllProfessors()
        {
            var users = await _userQuery.GetAllUser();

            var professors = new List<ProfessorDto>();

            foreach (var user in users) { 
                var roles = await _userManager.GetRolesAsync(user);

                if(roles.Contains("Professor") && user.personalClubProfile?.professorProfile != null){
                    professors.Add(new ProfessorDto
                    {
                        idUser = user.Id,
                        idPersonalClub = user.personalClubProfile.idPersonalClub,
                        idProfessor = user.personalClubProfile.professorProfile.idProfessor,
                        name = user.name,
                        lastName = user.lastName,
                        DNI = user.DNI,
                        userName = user.UserName!,
                        email = user.Email!,
                        phoneNumber = user.PhoneNumber!,
                        isActive = user.isActive,
                        roles = roles,
                        legajo = user.personalClubProfile.legajo,
                        startDate = user.personalClubProfile.startDate,
                        turno = user.personalClubProfile.turno,
                        speciality = user.personalClubProfile.professorProfile.speciality,
                        certification = user.personalClubProfile.professorProfile.certification
                    });
                }
            }

            if (professors.Count == 0)
                throw new NotFoundException("No se encontraron profesores");

            return professors;
        }
    }
}

