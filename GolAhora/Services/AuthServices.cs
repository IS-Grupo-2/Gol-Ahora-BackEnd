using GolAhora.Command;
using GolAhora.DTOs;
using GolAhora.Exceptions;
using GolAhora.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace GolAhora.Services
{
    public class AuthServices
    {
        private readonly ClientCommand _clientCommand;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthServices(ClientCommand clientCommand, UserManager<User> userManager, IConfiguration configuration)
        {
            _clientCommand = clientCommand;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> RegisterClient(RegisterClientDto dto)
        {
            var user = new User
            {
                name = dto.name,
                lastName = dto.lastName,
                DNI = dto.DNI,
                UserName = dto.userName,
                Email = dto.email,
                PhoneNumber = dto.phoneNumber,
                isActive = true,
                registerDate = DateTime.UtcNow
            };

            var existingUser = await _userManager.FindByNameAsync(dto.userName);
            if (existingUser != null)
                throw new BadRequestException("El nombre de usuario ya se encuentra registrado");

            var result = await _userManager.CreateAsync(user, dto.password);

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Client");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var profile = new ClientProfile
            {
                idUser = user.Id,
                numberPartner = dto.numberPartner,
                idTeam = dto.idTeam
            };

            await _clientCommand.AddClient(profile);

            return new CreatedResult("", new { user.Id, user.UserName, role = "Client"});
        }

        public async Task<IActionResult> RegisterEmployee(RegisterEmployeeDto dto)
        {
            var user = new User
            {
                name = dto.name,
                lastName = dto.lastName,
                DNI = dto.DNI,
                UserName = dto.userName,
                Email = dto.email,
                PhoneNumber = dto.phoneNumber,
                isActive = true,
                registerDate = DateTime.UtcNow
            };

            var existingUser = await _userManager.FindByNameAsync(dto.userName);
            if (existingUser != null)
                throw new BadRequestException("El nombre de usuario ya se encuentra registrado");

            var result = await _userManager.CreateAsync(user, dto.password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "PersonalClub");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult1 = await _userManager.AddToRoleAsync(user, "Employee");

            if (!roleResult1.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = dto.legajo,
                startDate = dto.startDate,
                turno = dto.turno
            };

            await _clientCommand.addPersonalClub(profile);

            var employee = new EmployeeProfile
            {
                idPersonalClub = profile.idPersonalClub,
                sector = dto.sector,
            };

            await _clientCommand.addEmployee(employee);

            return new CreatedResult("", new { user.Id, user.name, role = "Employee" });
        }

        public async Task<IActionResult> RegisterProfessor(RegisterProfessorDto dto)
        {
            var user = new User
            {
                name = dto.name,
                lastName = dto.lastName,
                DNI = dto.DNI,
                UserName = dto.userName,
                Email = dto.email,
                PhoneNumber = dto.phoneNumber,
                isActive = true,
                registerDate = DateTime.UtcNow
            };

            var existingUser = await _userManager.FindByNameAsync(dto.userName);
            if(existingUser != null)
                throw new BadRequestException("El nombre de usuario ya se encuentra registrado");

            var result = await _userManager.CreateAsync(user, dto.password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "PersonalClub");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var resultRole1 = await _userManager.AddToRoleAsync(user, "Professor"); 
            if (!resultRole1.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = dto.legajo,
                startDate = dto.startDate,
                turno = dto.turno
            };

            await _clientCommand.addPersonalClub(profile);

            var professor = new ProfessorProfile
            {
                idPersonalClub = profile.idPersonalClub,
                speciality = dto.specialty
            };

            await _clientCommand.addProfessor(professor);

            return new CreatedResult("", new { user.Id, user.name, role = "Professor" });
        }

        public async Task<LoginUserResponce?> LoginUser(LoginUserDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.userName);

            if (user == null)
                return null;

            if(!user.isActive)
              throw new BadRequestException("El usuario se encuentra inactivo.");

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.password);

            if (!validPassword)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var expiration = DateTime.UtcNow.AddHours(1);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!)
                );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new LoginUserResponce
            {
                token = token,
                expiration = expiration,
                user = new UserDto
                {
                    idUser = user.Id,
                    name = user.name,
                    lastName = user.lastName,
                    email = user.Email!,
                    phoneNumber = user.PhoneNumber!,
                    isActive = user.isActive,
                    roles = roles
                }
            };
        }
    }
}
