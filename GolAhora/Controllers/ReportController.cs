using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ReporteService _reporteService;

        public ReportesController(ReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        // RF57 – GET api/reportes/ingresos
        [HttpGet("ingresos")]
        public async Task<IActionResult> GenerarReporteIngresos([FromQuery] ReporteRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del reporte son invalidos.");
            var reporte = await _reporteService.GenerarReporteIngresos(dto);
            return Ok(reporte);
        }

        // RF58 – GET api/reportes/asistencia
        [HttpGet("asistencia")]
        public async Task<IActionResult> GenerarReporteAsistencia([FromQuery] ReporteRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del reporte son invalidos.");
            var reporte = await _reporteService.GenerarReporteAsistencia(dto);
            return Ok(reporte);
        }

        // RF59 – GET api/reportes/reservas
        [HttpGet("reservas")]
        public async Task<IActionResult> GenerarReporteReservas([FromQuery] ReporteRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del reporte son invalidos.");
            var reporte = await _reporteService.GenerarReporteReservas(dto);
            return Ok(reporte);
        }
    }
}