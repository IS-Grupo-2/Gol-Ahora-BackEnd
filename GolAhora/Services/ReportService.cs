using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GolAhora.Services
{
    public class ReporteService
    {
        private readonly GolAhora.Data.AppContext _context;

        public ReporteService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // ==========================================
        // RF57 – REPORTE DE INGRESOS TOTALES
        // ==========================================

        public async Task<ReporteIngresoDTO> GenerarReporteIngresos(int idAdmin, ReporteRequestDTO dto)
        {
            var admin = await _context.AdminProfiles.FindAsync(idAdmin);
            if (admin == null)
            {
                throw new NotFoundException("Admin no encontrado");
            }

            await _context.Entry(admin).Reference(a => a.personalClubProfile).LoadAsync();
            if (admin.personalClubProfile != null)
            {
                await _context.Entry(admin.personalClubProfile).Reference(p => p.user).LoadAsync();
            }

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
                generadoPor = admin.personalClubProfile?.user?.UserName ?? "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalIngresos = pagos.Sum(p => p.amount),
                ingresosPorConcepto = ingresosPorConcepto,
                cobros = cobros
            };
        }

        public async Task<byte[]> ImprimirReporteIngresos(int idAdmin, ReporteRequestDTO dto)
        {
            var reporte = await GenerarReporteIngresos(idAdmin, dto);

            var contenido =
                "==================================================\n" +
                "                    GOL AHORA                     \n" +
                "            REPORTE DE INGRESOS TOTALES           \n" +
                "==================================================\n" +
                $"Fecha de Generación: {reporte.fechaGeneracion:dd/MM/yyyy HH:mm}\n" +
                $"Generado por: {reporte.generadoPor}\n" +
                $"Período: {reporte.periodoDesde:dd/MM/yyyy} - {reporte.periodoHasta:dd/MM/yyyy}\n" +
                "--------------------------------------------------\n" +
                $"TOTAL INGRESOS: ${reporte.totalIngresos}\n" +
                "--------------------------------------------------\n" +
                "INGRESOS POR CONCEPTO:\n";

            foreach (var concepto in reporte.ingresosPorConcepto)
                contenido += $"  {concepto.Key}: ${concepto.Value}\n";

            contenido += "--------------------------------------------------\n" +
                "DETALLE DE COBROS:\n";

            foreach (var cobro in reporte.cobros)
                contenido +=
                    $"  ID: {cobro.idCobro} | Cliente: {cobro.clienteNombre} | " +
                    $"Monto: ${cobro.monto} | Fecha: {cobro.fechaPago:dd/MM/yyyy} | " +
                    $"Método: {cobro.metodoPago} | Estado: {(cobro.exitoso ? "Exitoso" : "Fallido")}\n";

            contenido +=
                "==================================================\n" +
                "              GOL AHORA - Control Interno         \n" +
                "==================================================\n";

            return Encoding.UTF8.GetBytes(contenido);
        }

        // ==========================================
        // RF58 – REPORTE DE ASISTENCIA TOTALES
        // ==========================================

        public async Task<ReporteAsistenciaDTO> GenerarReporteAsistencia(int idAdmin, ReporteRequestDTO dto)
        {
            var admin = await _context.AdminProfiles.FindAsync(idAdmin);
            if (admin == null)
            {
                throw new NotFoundException("Admin no encontrado");
            }

            await _context.Entry(admin).Reference(a => a.personalClubProfile).LoadAsync();
            if (admin.personalClubProfile != null)
            {
                await _context.Entry(admin.personalClubProfile).Reference(p => p.user).LoadAsync();
            }

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
                observaciones = a.observations // Corrección: Volvemos a 'observaciones' en español para el DTO
            }).ToList();

            var asistenciasPorClase = asistencias
                .GroupBy(a => a.clas.name)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReporteAsistenciaDTO
            {
                titulo = "Reporte de Asistencia",
                fechaGeneracion = DateTime.Now,
                generadoPor = admin.personalClubProfile?.user?.UserName ?? "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalAsistencias = asistencias.Count,
                asistenciasPorClase = asistenciasPorClase,
                asistencias = asistenciasDTO
            };
        }

        public async Task<byte[]> ImprimirReporteAsistencia(int idAdmin, ReporteRequestDTO dto)
        {
            var reporte = await GenerarReporteAsistencia(idAdmin, dto);

            var contenido =
                "==================================================\n" +
                "                    GOL AHORA                     \n" +
                "          REPORTE DE ASISTENCIA TOTALES           \n" +
                "==================================================\n" +
                $"Fecha de Generación: {reporte.fechaGeneracion:dd/MM/yyyy HH:mm}\n" +
                $"Generado por: {reporte.generadoPor}\n" +
                $"Período: {reporte.periodoDesde:dd/MM/yyyy} - {reporte.periodoHasta:dd/MM/yyyy}\n" +
                "--------------------------------------------------\n" +
                $"TOTAL ASISTENCIAS: {reporte.totalAsistencias}\n" +
                "--------------------------------------------------\n" +
                "ASISTENCIAS POR CLASE:\n";

            foreach (var clase in reporte.asistenciasPorClase)
                contenido += $"  {clase.Key}: {clase.Value}\n";

            contenido += "--------------------------------------------------\n" +
                "DETALLE DE ASISTENCIAS:\n";

            foreach (var asistencia in reporte.asistencias)
                contenido +=
                    $"  ID: {asistencia.id} | Cliente: {asistencia.clienteNombre} | " +
                    $"Clase: {asistencia.claseNombre} | " +
                    $"Presente: {(asistencia.presente ? "Sí" : "No")} | " +
                    $"Observaciones: {asistencia.observaciones}\n"; // Corrección aquí también

            contenido +=
                "==================================================\n" +
                "              GOL AHORA - Control Interno         \n" +
                "==================================================\n";

            return Encoding.UTF8.GetBytes(contenido);
        }

        // ==========================================
        // RF59 – REPORTE DE RESERVAS TOTALES
        // ==========================================

        public async Task<ReporteReservaDTO> GenerarReporteReservas(int idAdmin, ReporteRequestDTO dto)
        {
            var admin = await _context.AdminProfiles.FindAsync(idAdmin);
            if (admin == null)
            {
                throw new NotFoundException("Admin no encontrado");
            }

            await _context.Entry(admin).Reference(a => a.personalClubProfile).LoadAsync();
            if (admin.personalClubProfile != null)
            {
                await _context.Entry(admin.personalClubProfile).Reference(p => p.user).LoadAsync();
            }

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
                generadoPor = admin.personalClubProfile?.user?.UserName ?? "Admin",
                periodoDesde = dto.periodoDesde,
                periodoHasta = dto.periodoHasta,
                totalReservas = reservas.Count,
                reservasPorCanchas = reservasPorCanchas,
                reservasPorEstado = reservasPorEstado,
                reservas = reservasDTO
            };
        }

        public async Task<byte[]> ImprimirReporteReservas(int idAdmin, ReporteRequestDTO dto)
        {
            var reporte = await GenerarReporteReservas(idAdmin, dto);

            var contenido =
                "==================================================\n" +
                "                    GOL AHORA                     \n" +
                "           REPORTE DE RESERVAS TOTALES            \n" +
                "==================================================\n" +
                $"Fecha de Generación: {reporte.fechaGeneracion:dd/MM/yyyy HH:mm}\n" +
                $"Generado por: {reporte.generadoPor}\n" +
                $"Período: {reporte.periodoDesde:dd/MM/yyyy} - {reporte.periodoHasta:dd/MM/yyyy}\n" +
                "--------------------------------------------------\n" +
                $"TOTAL RESERVAS: {reporte.totalReservas}\n" +
                "--------------------------------------------------\n" +
                "RESERVAS POR CANCHA:\n";

            foreach (var cancha in reporte.reservasPorCanchas)
                contenido += $"  {cancha.Key}: {cancha.Value}\n";

            contenido += "RESERVAS POR ESTADO:\n";

            foreach (var estado in reporte.reservasPorEstado)
                contenido += $"  {estado.Key}: {estado.Value}\n";

            contenido += "--------------------------------------------------\n" +
                "DETALLE DE RESERVAS:\n";

            foreach (var reserva in reporte.reservas)
                contenido +=
                    $"  ID: {reserva.id} | Cliente: {reserva.clienteNombre} | " +
                    $"Cancha: {reserva.canchaNombre} | Fecha: {reserva.fechaReserva:dd/MM/yyyy} | " +
                    $"Inicio: {reserva.horaInicio} | Fin: {reserva.horaFin} | " +
                    $"Pagado: {(reserva.pagado ? "Sí" : "No")} | Total: ${reserva.precioTotal}\n";

            contenido +=
                "==================================================\n" +
                "              GOL AHORA - Control Interno         \n" +
                "==================================================\n";

            return Encoding.UTF8.GetBytes(contenido);
        }
    }
}
