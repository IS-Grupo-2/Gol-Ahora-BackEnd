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
                return BadRequest("Los datos de disponibilidad son invalidos.");

            var (success, message) = await _disponibilityService.AgregarDisponibility(dto);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // PUT api/disponibilities/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarDisponibility(int id, [FromBody] DisponibilityDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de disponibilidad son invalidos.");

            var (success, message) = await _disponibilityService.ModificarDisponibility(id, dto);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // GET api/disponibilities
        [HttpGet]
        public async Task<IActionResult> ListarDisponibilities()
        {
            var disponibilities = await _disponibilityService.ListarDisponibilities();
            return Ok(disponibilities);
        }

        // GET api/disponibilities/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarDisponibility(int id)
        {
            var disponibility = await _disponibilityService.ConsultarDisponibility(id);
            if (disponibility == null)
                return NotFound($"No se encontro la disponibilidad con ID {id}.");

            return Ok(disponibility);
        }

        // RF18 – PATCH api/disponibilities/{id}/habilitar
        [HttpPatch("{id}/habilitar")]
        public async Task<IActionResult> HabilitarDisponibility(int id)
        {
            var (success, message) = await _disponibilityService.HabilitarDisponibility(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF18 – PATCH api/disponibilities/{id}/deshabilitar
        [HttpPatch("{id}/deshabilitar")]
        public async Task<IActionResult> DeshabilitarDisponibility(int id)
        {
            var (success, message) = await _disponibilityService.DeshabilitarDisponibility(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }
    }
}
