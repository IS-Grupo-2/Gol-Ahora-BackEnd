// CourtsController.cs
using GolAhora.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtsController : ControllerBase
    {
        private readonly CourtService _courtService;

        public CourtsController(CourtService courtService)
        {
            _courtService = courtService;
        }

        // RF13 – GET api/courts
        [HttpGet]
        public async Task<IActionResult> ListarCourts()
        {
            var courts = await _courtService.ListarCourts();
            return Ok(courts);
        }

        // RF14 – DELETE api/courts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBajaCourt(int id)
        {
            var (success, message) = await _courtService.DarDeBajaCourt(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF15 – GET api/courts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarCourt(int id)
        {
            var court = await _courtService.ConsultarCourt(id);
            if (court == null)
                return NotFound($"No se encontró la cancha con ID {id}.");

            return Ok(court);
        }

        // RF18 – PATCH api/courts/disponibility/{id}/habilitar
        [HttpPatch("disponibility/{id}/habilitar")]
        public async Task<IActionResult> HabilitarDisponibility(int id)
        {
            var (success, message) = await _courtService.HabilitarDisponibility(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }
    }
}