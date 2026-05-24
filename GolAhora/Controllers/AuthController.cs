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

        [HttpPost("register/client")]
        public async Task<IActionResult> RegisterClient([FromBody]RegisterClientDto dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cliente son invalidos.");

            var result = await _authServices.RegisterClient(dto);
            return new JsonResult(result) { StatusCode = 201 };
        }
    }
}
