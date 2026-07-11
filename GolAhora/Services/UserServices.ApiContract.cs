using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class UserServices
    {

        public async Task<IActionResult> GetClientes()
        {
            var users = await _context.Users
                .Include(u => u.clientProfile)
                .Where(u => u.clientProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idCliente = u.clientProfile!.idClient,
                idClient = u.clientProfile.idClient,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "cliente",
                nroSocio = $"C-{u.clientProfile.numberPartner:000}",
                numberPartner = u.clientProfile.numberPartner,
                idTeam = u.clientProfile.idTeam
            }));
        }

        public async Task<IActionResult> CreateCliente([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "Client");
            var partnerNumber = await _context.ClientProfiles.CountAsync() + 1;
            _context.ClientProfiles.Add(new ClientProfile { idUser = user.Id, numberPartner = partnerNumber });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        public async Task<IActionResult> UpdateCliente(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        public async Task<IActionResult> ToggleCliente(int id) => await ToggleUser(id);

        public async Task<IActionResult> GetEmpleados()
        {
            var users = await _context.Users
                .Include(u => u.personalClubProfile)!.ThenInclude(p => p!.employeeProfile)
                .Where(u => u.personalClubProfile != null && u.personalClubProfile.employeeProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idEmployee = u.personalClubProfile!.employeeProfile!.idEmployee,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "empleado",
                legajo = u.personalClubProfile.legajo,
                turno = u.personalClubProfile.turno,
                sector = u.personalClubProfile.employeeProfile.sector
            }));
        }

        public async Task<IActionResult> CreateEmpleado([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);
            validation = ValidateStaffPayload(body, requireSector: true);
            if (validation is not null) return ValidationError(validation);

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "PersonalClubProfile");
            await EnsureRole(user, "Employee");
            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = ReadString(body, "legajo") ?? $"E-{user.Id:000}",
                startDate = ReadDate(body, "startDate") ?? DateTime.UtcNow,
                turno = ReadString(body, "turno") ?? ""
            };
            _context.PersonalClubProfiles.Add(profile);
            await _unitOfWork.SaveChangesAsync();
            _context.EmployeeProfiles.Add(new EmployeeProfile { idPersonalClub = profile.idPersonalClub, sector = ReadString(body, "sector") ?? "" });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        public async Task<IActionResult> UpdateEmpleado(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        public async Task<IActionResult> ToggleEmpleado(int id) => await ToggleUser(id);

        public async Task<IActionResult> GetProfesores()
        {
            var users = await _context.Users
                .Include(u => u.personalClubProfile)!.ThenInclude(p => p!.professorProfile)!.ThenInclude(p => p!.certifications)
                .Where(u => u.personalClubProfile != null && u.personalClubProfile.professorProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idProfessor = u.personalClubProfile!.professorProfile!.idProfessor,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "profesor",
                legajo = u.personalClubProfile.legajo,
                turno = u.personalClubProfile.turno,
                especialidad = u.personalClubProfile.professorProfile.speciality,
                speciality = u.personalClubProfile.professorProfile.speciality,
                certification = u.personalClubProfile.professorProfile.certification,
                certificaciones = u.personalClubProfile.professorProfile.certifications.Select(c => new
                {
                    id = c.idCertification,
                    nombre = c.name,
                    institucion = c.institution,
                    numero = c.numberCertificate,
                    fecha = c.dateObtained,
                    verificada = c.verified
                })
            }));
        }

        public async Task<IActionResult> CreateProfesor([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);
            validation = ValidateStaffPayload(body, requireSector: false);
            if (validation is not null) return ValidationError(validation);
            if ((ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? "").Trim().Length < 2) return ValidationError("Especialidad invalida.");

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "PersonalClubProfile");
            await EnsureRole(user, "Professor");
            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = ReadString(body, "legajo") ?? $"P-{user.Id:000}",
                startDate = ReadDate(body, "startDate") ?? DateTime.UtcNow,
                turno = ReadString(body, "turno") ?? ""
            };
            _context.PersonalClubProfiles.Add(profile);
            await _unitOfWork.SaveChangesAsync();
            _context.ProfessorProfiles.Add(new ProfessorProfile
            {
                idPersonalClub = profile.idPersonalClub,
                speciality = ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? "",
                certification = ReadString(body, "certification") ?? ""
            });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        public async Task<IActionResult> UpdateProfesor(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        public async Task<IActionResult> ToggleProfesor(int id) => await ToggleUser(id);

        public async Task<IActionResult> ChangePassword(int id, [FromBody] JsonElement body)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            var currentPassword = ReadString(body, "currentPassword") ?? ReadString(body, "passwordActual") ?? "";
            var newPassword = ReadString(body, "newPassword") ?? ReadString(body, "nuevaPassword") ?? "";
            var confirmPassword = ReadString(body, "confirmPassword") ?? ReadString(body, "confirmarPassword") ?? newPassword;

            if (string.IsNullOrWhiteSpace(currentPassword)) return ValidationError("Ingrese la contrasena actual.");
            if (newPassword.Length < 6) return ValidationError("La nueva contrasena debe tener al menos 6 caracteres.");
            if (newPassword != confirmPassword) return ValidationError("La confirmacion no coincide con la nueva contrasena.");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new { idUsuario = id });
        }

        public IActionResult SendSupportMessage([FromBody] JsonElement body)
        {
            var message = ReadString(body, "mensaje") ?? ReadString(body, "message") ?? "";
            if (message.Trim().Length < 10) return ValidationError("El mensaje debe tener al menos 10 caracteres.");

            return Ok(new { enviado = true });
        }
    }
}


