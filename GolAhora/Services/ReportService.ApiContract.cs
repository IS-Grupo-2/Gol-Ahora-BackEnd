using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class ReporteService
    {

        public async Task<IActionResult> ReporteIngresos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var from = desde ?? DateTime.MinValue;
            var to = hasta ?? DateTime.MaxValue;
            var pagos = await _context.Payments.Where(p => p.paymentDate >= from && p.paymentDate <= to && p.isSuccessful).ToListAsync();
            var total = pagos.Sum(p => p.amount);
            return Ok(new
            {
                totalIngresos = total,
                ingresosPorConcepto = pagos.GroupBy(p => p.paymentMethod).Select(g => new { concepto = g.Key, monto = g.Sum(p => p.amount), porcentaje = total == 0 ? 0 : Math.Round(g.Sum(p => p.amount) * 100 / total, 2), color = "fill-green" })
            });
        }

        public async Task<IActionResult> ReporteAsistencias([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var asistencias = await _context.Assistances.Include(a => a.clas).ToListAsync();
            return Ok(new
            {
                totalAsistencias = asistencias.Count,
                asistenciasPorClase = asistencias.GroupBy(a => a.clas.name).Select(g => new { clase = g.Key, asistentes = g.Count(a => a.isAssisted), capacidad = g.Count(), porcentaje = g.Count() == 0 ? 0 : Math.Round(g.Count(a => a.isAssisted) * 100.0 / g.Count(), 2), color = "fill-blue" })
            });
        }

        public async Task<IActionResult> ReporteReservas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var from = desde ?? DateTime.MinValue;
            var to = hasta ?? DateTime.MaxValue;
            var reservas = await _context.Reservations
                .Include(r => r.court)
                .Where(r => r.reservationDate >= from && r.reservationDate <= to)
                .ToListAsync();

            return Ok(new
            {
                totalReservas = reservas.Count,
                reservasPorCancha = reservas.GroupBy(r => r.court.name).Select(g => new
                {
                    cancha = g.Key,
                    cantidad = g.Count(),
                    ingresos = g.Sum(r => r.totalPrice),
                    porcentaje = reservas.Count == 0 ? 0 : Math.Round(g.Count() * 100.0 / reservas.Count, 2),
                    color = "fill-orange"
                })
            });
        }
    }
}



