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

        // POST: api/assistance/clase
        [HttpPost("clase/{idClass}")]
        public async Task<IActionResult> RegistrarAsistencia(int idClass, [FromBody] List<AssistanceDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest("La lista de asistencia no contiene datos válidos.");

            foreach (var dto in dtos)
            {
                var result = await _assistanceService.RegistrarAsistencia(dto);
                if (!result.Item1)
                    return BadRequest(new { mensaje = result.Item2 });
            }

            return Ok(new { mensaje = "Asistencias registradas correctamente." });
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
        // GET api/assistance
        [HttpGet]
        public async Task<IActionResult> ListarAsistencias()
        {
            var list = await _assistanceService.ListarAsistencias();
            return Ok(list);
        }

        // GET api/assistance/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarAsistencia(int id)
        {
            var asistencia = await _assistanceService.ConsultarAsistencia(id);
            if (asistencia == null) return NotFound($"No se encontró la asistencia con ID {id}.");
            return Ok(asistencia);
        }

        public class AssistanceModifyDTO
        {
            public bool isAssisted { get; set; }
            public string? observations { get; set; }
        }
    }
}