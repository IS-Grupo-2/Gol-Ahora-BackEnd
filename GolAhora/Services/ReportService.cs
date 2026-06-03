using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ReporteService
    {
        private readonly GolAhora.Data.AppContext _context;

        public ReporteService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF57 – Generar reporte de ingresos totales
        public async Task<ReporteIngresoDTO> GenerarReporteIngresos(ReporteRequestDTO dto)
        {
            var pagos = await _context.Payments
                .Include(p => p.client)
                    .ThenInclude(c => c.user)
                .Where(p => p.paymentDate >= dto.periodoDesde && p.paymentDate <= dto.periodoHasta)
                .ToListAsync();

            var cobros = pagos.Select(p => new CobrosDTO
            {
                idCobro = p.idPayment,
                clienteNombre = p.client.user.name + " " + p.client.user.lastName,
                monto = p.amount,
                fechaPago = p.paymentDate,
                metodoPago = p.paymentMethod,
                exitoso = p.isSuccessful
            }).ToList();

            var ingresosPorConcepto = pagos
                .GroupBy(p => p.paymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.amount));

            return new ReporteIngresoDTO
            {
                titulo = "Reporte de Ingresos",
                fechaGeneracion = DateTime.Now,
                generadoPor = "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalIngresos = pagos.Sum(p => p.amount),
                ingresosPorConcepto = ingresosPorConcepto,
                cobros = cobros
            };
        }

        // RF58 – Generar reporte de asistencia totales
        public async Task<ReporteAsistenciaDTO> GenerarReporteAsistencia(ReporteRequestDTO dto)
        {
            var asistencias = await _context.Assistances
                .Include(a => a.client)
                    .ThenInclude(c => c.user)
                .Include(a => a.clas)
                .ToListAsync();

            var asistenciasDTO = asistencias.Select(a => new AsistenciaDTO
            {
                id = a.idAssistance,
                clienteNombre = a.client.user.name + " " + a.client.user.lastName,
                claseNombre = a.clas.name,
                presente = a.isAssisted,
                observaciones = a.observations
            }).ToList();

            var asistenciasPorClase = asistencias
                .GroupBy(a => a.clas.name)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReporteAsistenciaDTO
            {
                titulo = "Reporte de Asistencia",
                fechaGeneracion = DateTime.Now,
                generadoPor = "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalAsistencias = asistencias.Count,
                asistenciasPorClase = asistenciasPorClase,
                asistencias = asistenciasDTO
            };
        }

        // RF59 – Generar reporte de reservas totales
        public async Task<ReporteReservaDTO> GenerarReporteReservas(ReporteRequestDTO dto)
        {
            var reservas = await _context.Reservations
                .Include(r => r.client)
                    .ThenInclude(c => c.user)
                .Include(r => r.court)
                .Where(r => r.reservationDate >= dto.periodoDesde && r.reservationDate <= dto.periodoHasta)
                .ToListAsync();

            var reservasDTO = reservas.Select(r => new ReservaResumenDTO
            {
                id = r.idReservation,
                clienteNombre = r.client.user.name + " " + r.client.user.lastName,
                canchaNombre = r.court.name,
                fechaReserva = r.reservationDate,
                horaInicio = r.startTime,
                horaFin = r.endTime,
                pagado = r.isPaid,
                precioTotal = r.totalPrice
            }).ToList();

            var reservasPorCanchas = reservas
                .GroupBy(r => r.court.name)
                .ToDictionary(g => g.Key, g => g.Count());

            var reservasPorEstado = reservas
                .GroupBy(r => r.isPaid ? "Pagada" : "Pendiente")
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReporteReservaDTO
            {
                titulo = "Reporte de Reservas",
                fechaGeneracion = DateTime.Now,
                generadoPor = "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalReservas = reservas.Count,
                reservasPorCanchas = reservasPorCanchas,
                reservasPorEstado = reservasPorEstado,
                reservas = reservasDTO
            };
        }
    }
}