using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppDbContext = GolAhora.Data.AppContext;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthServices _authServices;

        public AuthController(AppDbContext context, AuthServices authServices)
        {
            _context = context;
            _authServices = authServices;
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
        {
            if(dto == null)
                return BadRequest("Los datos son invalidos.");

            var result = await _authServices.RegisterAdmin(dto);
            return new JsonResult(result) { StatusCode = 201 };
        }

        [HttpPost("register/client")]
        public async Task<IActionResult> RegisterClient([FromBody]RegisterClientDto dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cliente son invalidos.");

            var result = await _authServices.RegisterClient(dto);
            return new JsonResult(result) { StatusCode = 201 };
        }

        [HttpPost("register/employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeDto dto)
        {
            if (dto == null)
                return BadRequest("Los datos del empleado son invalidos");

            var result = await _authServices.RegisterEmployee(dto);
            return new JsonResult(result) { StatusCode = 201 };

        }

        [HttpPost("register/professor")]
        public async Task<IActionResult> RegisterProfessor([FromBody] RegisterProfessorDto dto)
        {
            if (dto == null)
                return BadRequest("Los datos del profesor");

            var result = await _authServices.RegisterProfessor(dto);
            return new JsonResult(result) { StatusCode = 201 };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (dto == null)
                return BadRequest("Debe completar todos los campos");

            var result = await _authServices.LoginUser(dto);

            if (result == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            return Ok(result);
        }
    }
}

