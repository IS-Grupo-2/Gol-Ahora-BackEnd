using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtTypesController : ControllerBase
    {
        private readonly CourtTypeService _courtTypeService;

        public CourtTypesController(CourtTypeService courtTypeService)
        {
            _courtTypeService = courtTypeService;
        }

        // RF11 – POST api/courttypes
        [HttpPost]
        public async Task<IActionResult> AgregarCourtType([FromBody] CourtTypeDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del tipo de cancha son inválidos.");

            var (success, message) = await _courtTypeService.AgregarCourtType(dto);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF12 – PUT api/courttypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarCourtType(int id, [FromBody] CourtTypeDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del tipo de cancha son inválidos.");

            var (success, message) = await _courtTypeService.ModificarCourtType(id, dto);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF13 – GET api/courttypes
        [HttpGet]
        public async Task<IActionResult> ListarCourtTypes()
        {
            var courtTypes = await _courtTypeService.ListarCourtTypes();
            return Ok(courtTypes);
        }

        // RF15 – GET api/courttypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarCourtType(int id)
        {
            var courtType = await _courtTypeService.ConsultarCourtType(id);
            if (courtType == null)
                return NotFound($"No se encontró el tipo de cancha con ID {id}.");

            return Ok(courtType);
        }

        // RF14 – DELETE api/courttypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCourtType(int id)
        {
            var (success, message) = await _courtTypeService.EliminarCourtType(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }
    }
}