using GolAhora.Command;
using GolAhora.DTOs;
using GolAhora.Exceptions;
using GolAhora.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


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
    }
}
