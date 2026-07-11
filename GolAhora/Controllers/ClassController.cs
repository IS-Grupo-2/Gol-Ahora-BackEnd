using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly ClassService _classService;
        private readonly ReservationService _reservationService;

        public ClassesController(ClassService classService, ReservationService reservationService)
        {
            _classService = classService;
            _reservationService = reservationService;
        }

        // RF35 – POST api/classes
        [HttpPost]
        public async Task<IActionResult> ProgramarClase([FromBody] ClassDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos para programar la clase son inválidos." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (success, message, idClass) = await _classService.ProgramarClase(dto);
            if (!success) return BadRequest(new { mensaje = message });
            return Ok(new { mensaje = message, idClass });
        }

        // Modificar clase - PUT api/classes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarClase(int id, [FromBody] ClassDTO dto)
        {
            if (id <= 0) return BadRequest(new { mensaje = "Id de clase inválido." });
            if (dto == null) return BadRequest(new { mensaje = "Datos de clase inválidos." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (success, message) = await _classService.ModificarClase(id, dto);
            if (!success) return BadRequest(new { mensaje = message });
            return Ok(new { mensaje = message });
        }

        // Cancelar clase - PUT api/classes/{id}/cancelar
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarClase(int id)
        {
            if (id <= 0) return BadRequest(new { mensaje = "Id de clase inválido." });

            var (success, message) = await _classService.CancelarClase(id);
            if (!success) return BadRequest(new { mensaje = message });
            return Ok(new { mensaje = message });
        }

        // Consultar clase - GET api/classes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarClase(int id)
        {
            if (id <= 0) return BadRequest(new { mensaje = "Id de clase inválido." });

            var dto = await _classService.ConsultarClase(id);
            if (dto == null) return NotFound(new { mensaje = "La clase no existe." });
            return Ok(dto);
        }

        // Agregar alumno
        [HttpPost("{id}/agregar-alumno/{clientId}")]
        public async Task<IActionResult> AgregarAlumno(int id, int clientId)
        {
            if (id <= 0) return BadRequest(new { mensaje = "Id de clase inválido." });
            if (clientId <= 0) return BadRequest(new { mensaje = "Id de cliente inválido." });

            var (success, message) = await _classService.AgregarAlumno(id, clientId);
            if (!success) return BadRequest(new { mensaje = message });
            return Ok(new { mensaje = message });
        }
        [HttpPost("{idClass}/asistencia")]
        public async Task<IActionResult> RegistrarAsistencia(int idClass, [FromBody] List<AssistanceDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest("La lista de asistencia no contiene datos válidos.");


            var (success, message) = await _classService.RegistrarAsistencia(idClass, dtos);

            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // GET api/classes
        [HttpGet]
        public async Task<IActionResult> ListarClases()
        {
            var clases = await _classService.ListarClases();
            return Ok(clases);
        }
        [HttpGet("/api/clases")]
        public Task<IActionResult> GetClasesApiContract() => _reservationService.GetClases();

        [HttpPost("/api/clases")]
        public Task<IActionResult> CreateClaseApiContract([FromBody] JsonElement body) => _reservationService.CreateClase(body);

        [HttpPut("/api/clases/{id}")]
        public Task<IActionResult> UpdateClaseApiContract(int id, [FromBody] JsonElement body) => _reservationService.UpdateClase(id, body);

        [HttpPut("/api/clases/{id}/cancelar")]
        public Task<IActionResult> CancelarClaseApiContract(int id) => _reservationService.CancelarClase(id);

        [HttpPost("/api/clases/{id}/agregar-alumno/{clientId}")]
        public Task<IActionResult> AddAlumnoApiContract(int id, int clientId) => _reservationService.AddAlumno(id, clientId);

        [HttpPost("/api/clases/{id}/asistencia")]
        public Task<IActionResult> SaveClaseAsistenciaApiContract(int id, [FromBody] JsonElement body) => _reservationService.SaveAsistencia(id, body);    }
}



