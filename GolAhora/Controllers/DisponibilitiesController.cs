// DisponibilitiesController.cs
using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisponibilitiesController : ControllerBase
    {
        private readonly DisponibilityService _disponibilityService;

        public DisponibilitiesController(DisponibilityService disponibilityService)
        {
            _disponibilityService = disponibilityService;
        }

        // RF16 – POST api/disponibilities
        [HttpPost]
        public async Task<IActionResult> AgregarDisponibility([FromBody] DisponibilityDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de disponibilidad son inválidos.");

            var (success, message) = await _disponibilityService.AgregarDisponibility(dto);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }
    }
}