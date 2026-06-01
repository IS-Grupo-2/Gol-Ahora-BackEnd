using System;
using GolAhora.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolAhora.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly AssistanceService _assistanceService;

        // recibe el servicio de asistencias en el constructor
        public AssistanceController(AssistanceService assistanceService)
        {
            _assistanceService = assistanceService;
        }

        // RF40 – POST api/assistance
        [HttpPost]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] AssistanceDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la asistencia son inválidos.");

            var (success, message) = await _assistanceService.RegistrarAsistencia(dto);
            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF41 – PUT api/assistance/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarAsistencia(int id, [FromBody] AssistanceDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la asistencia son inválidos.");

            var (success, message) = await _assistanceService.ModificarAsistencia(id, dto);
            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF42 – GET api/assistance/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarAsistencia(int id)
        {
            var assistance = await _assistanceService.ConsultarAsistencia(id);
            if (assistance == null)
                return NotFound($"No se encontró el registro de asistencia con ID {id}.");

            return Ok(assistance);
        }

        // RF43 – GET api/assistance
        [HttpGet]
        public async Task<IActionResult> ListarAsistencias()
        {
            var lista = await _assistanceService.ListarAsistencias();
            return Ok(lista);
        }
    }
}
