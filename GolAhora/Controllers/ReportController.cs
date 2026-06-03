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

        [HttpGet("ingresos/print")]
        public async Task<IActionResult> ImprimirReporteIngresos([FromQuery] int idAdmin, [FromQuery] ReporteRequestDTO dto)
        {
            var bytes = await _reporteService.ImprimirReporteIngresos(idAdmin, dto);
            var stream = new MemoryStream(bytes);
            return File(stream, "text/plain", "Reporte_Ingresos.txt");
        }

        [HttpGet("asistencia/print")]
        public async Task<IActionResult> ImprimirReporteAsistencia([FromQuery] int idAdmin, [FromQuery] ReporteRequestDTO dto)
        {
            // Corrección: Ahora pasamos el idAdmin al servicio
            var bytes = await _reporteService.ImprimirReporteAsistencia(idAdmin, dto);
            var stream = new MemoryStream(bytes);
            return File(stream, "text/plain", "Reporte_Asistencia.txt");
        }

        [HttpGet("reservas/print")]
        public async Task<IActionResult> ImprimirReporteReservas([FromQuery] int idAdmin, [FromQuery] ReporteRequestDTO dto)
        {
            // Corrección: Ahora pasamos el idAdmin al servicio
            var bytes = await _reporteService.ImprimirReporteReservas(idAdmin, dto);
            var stream = new MemoryStream(bytes);
            return File(stream, "text/plain", "Reporte_Reservas.txt");
        }
    }
}