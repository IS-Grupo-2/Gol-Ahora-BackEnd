using GolAhora.DTOs;
using GolAhora.Services;
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

        // POST api/courts
        [HttpPost]
        public async Task<IActionResult> AgregarCourt([FromBody] CourtDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la cancha son invalidos.");

            var (success, message) = await _courtService.AgregarCourt(dto);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // PUT api/courts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarCourt(int id, [FromBody] CourtDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la cancha son invalidos.");

            var (success, message) = await _courtService.ModificarCourt(id, dto);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
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
                return NotFound($"No se encontro la cancha con ID {id}.");

            return Ok(court);
        }

        // disponible(fecha, hora) – GET api/courts/{id}/disponible
        [HttpGet("{id}/disponible")]
        public async Task<IActionResult> ConsultarDisponibilidadCancha(int id, [FromQuery] DateTime fecha, [FromQuery] TimeSpan hora)
        {
            var (success, message) = await _courtService.ConsultarDisponibilidadCancha(id, fecha, hora);
            return Ok(new { disponible = success, mensaje = message });
        }
    }
}
