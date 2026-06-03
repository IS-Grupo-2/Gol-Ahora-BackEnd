using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GolAhora.Controllers
{
    [Authorize]
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

            if(!result.isActive)
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
            return new JsonResult(result) {  StatusCode = 200 };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);
            return new JsonResult(result) { StatusCode = 201 };
        }
    }
}
