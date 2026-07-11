using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Claims;

namespace GolAhora.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserServices _userService;

        public UserController(UserServices userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (idClaim == null || !int.TryParse(idClaim, out var id))
                return Unauthorized("Token invalido");

            var result = await _userService.Me(id);

            if (!result.isActive)
                return Unauthorized("Usuario inactivo");

            return new JsonResult(result) { StatusCode = 201 };
        }

        [HttpGet]
        public async Task<IActionResult> GetallUsers()
        {
            var result = await _userService.GetAllUsers();
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getUserById(int id)
        {
            var result = await _userService.GetUserById(id);
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateUser(int id, UpdateUserDTO upUser)
        {
            var result = await _userService.UpdateUser(id, upUser);
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpGet("/Users/Clients")]
        public async Task<IActionResult> GetAllClients()
        {
            var result = await _userService.GetAllClients();
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpGet("/Users/Employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var result = await _userService.GetAllEmployees();
            return new JsonResult(result) { StatusCode = 200 };

        }

        [HttpGet("/Users/Professors")]
        public async Task<IActionResult> GetAllProfessors()
        {
            var result = await _userService.GetAllProfessors();
            return new JsonResult(result) { StatusCode = 200 };

        }

        [HttpGet("/api/clientes")]
        public Task<IActionResult> GetClientesApiContract() => _userService.GetClientes();

        [HttpPost("/api/clientes")]
        public Task<IActionResult> CreateClienteApiContract([FromBody] JsonElement body) => _userService.CreateCliente(body);

        [HttpPut("/api/clientes/{id}")]
        public Task<IActionResult> UpdateClienteApiContract(int id, [FromBody] JsonElement body) => _userService.UpdateCliente(id, body);

        [HttpPatch("/api/clientes/{id}/estado")]
        [HttpPut("/api/clientes/{id}/estado")]
        public Task<IActionResult> ToggleClienteApiContract(int id) => _userService.ToggleCliente(id);

        [HttpGet("/api/empleados")]
        public Task<IActionResult> GetEmpleadosApiContract() => _userService.GetEmpleados();

        [HttpPost("/api/empleados")]
        public Task<IActionResult> CreateEmpleadoApiContract([FromBody] JsonElement body) => _userService.CreateEmpleado(body);

        [HttpPut("/api/empleados/{id}")]
        public Task<IActionResult> UpdateEmpleadoApiContract(int id, [FromBody] JsonElement body) => _userService.UpdateEmpleado(id, body);

        [HttpPatch("/api/empleados/{id}/estado")]
        [HttpPut("/api/empleados/{id}/estado")]
        public Task<IActionResult> ToggleEmpleadoApiContract(int id) => _userService.ToggleEmpleado(id);

        [HttpGet("/api/profesores")]
        public Task<IActionResult> GetProfesoresApiContract() => _userService.GetProfesores();

        [HttpPost("/api/profesores")]
        public Task<IActionResult> CreateProfesorApiContract([FromBody] JsonElement body) => _userService.CreateProfesor(body);

        [HttpPut("/api/profesores/{id}")]
        public Task<IActionResult> UpdateProfesorApiContract(int id, [FromBody] JsonElement body) => _userService.UpdateProfesor(id, body);

        [HttpPatch("/api/profesores/{id}/estado")]
        [HttpPut("/api/profesores/{id}/estado")]
        public Task<IActionResult> ToggleProfesorApiContract(int id) => _userService.ToggleProfesor(id);

        [HttpPut("/api/usuarios/{id}/password")]
        [HttpPut("/api/User/{id}/password")]
        public Task<IActionResult> ChangePasswordApiContract(int id, [FromBody] JsonElement body) => _userService.ChangePassword(id, body);

        [HttpPost("/api/soporte")]
        public IActionResult SendSupportMessageApiContract([FromBody] JsonElement body) => _userService.SendSupportMessage(body);
    }
}



