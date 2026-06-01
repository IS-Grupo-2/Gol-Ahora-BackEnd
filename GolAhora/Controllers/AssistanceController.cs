using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly AssistanceService _assistanceService;

        public AssistanceController(AssistanceService assistanceService)
        {
            _assistanceService = assistanceService;
        }

        // POST: api/assistance/clase/5
        [HttpPost("clase/{idClass}")]
        public async Task<IActionResult> RegistrarAsistencia(int idClass, [FromBody] List<AssistanceDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest("La lista de asistencia no contiene datos válidos.");

            var (success, message) = await _assistanceService.RegistrarAsistenciasClase(idClass, dtos);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // PUT: api/assistance/5
        [HttpPut("{idAssistance}")]
        public async Task<IActionResult> ModificarAsistencia(int idAssistance, [FromBody] bool presente, [FromQuery] string observaciones = "")
        {
            var (success, message) = await _assistanceService.ModificarAsistencia(idAssistance, presente, observaciones);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }
    }
}