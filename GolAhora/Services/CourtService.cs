using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class CourtService
    {
        private readonly GolAhora.Data.AppContext _context;

        public CourtService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        private async Task<bool> EsDisponible(int courtId, DateTime fecha, TimeSpan hora)
        {
            return await _context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == fecha.DayOfWeek &&
                d.startTime <= hora &&
                d.endTime >= hora &&
                d.isAvailable
            );
        }

        public async Task<(bool success, string message)> AgregarCourt(CourtDTO dto)
        {
            var courtType = await _context.CourtTypes.FindAsync(dto.courtTypeId);

            if (courtType == null)
                return (false, "El tipo de cancha no existe.");

            var court = new Court
            {
                name = dto.name,
                description = dto.description ?? "",
                imageUrl = "",
                courtTypeId = dto.courtTypeId,
                isAvailable = true
            };

            _context.Courts.Add(court);

            await _context.SaveChangesAsync();

            return (true, "Cancha registrada exitosamente.");
        }

        public async Task<(bool success, string message)> ModificarCourt(int id, CourtDTO dto)
        {
            var court = await _context.Courts.FindAsync(id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.name = dto.name;
            court.description = dto.description ?? "";
            court.courtTypeId = dto.courtTypeId;

            await _context.SaveChangesAsync();

            return (true, "Cancha modificada exitosamente.");
        }

        public async Task<List<CourtResponseDTO>> ListarCourts()
        {
            return await _context.Courts
                .Include(c => c.courtType)
                .Select(c => new CourtResponseDTO
                {
                    idCourt = c.idCourt,
                    name = c.name,
                    isAvailable = c.isAvailable,
                    description = c.description,
                    courtTypeName = c.courtType.name
                })
                .ToListAsync();
        }

        // BAJA LÓGICA
        public async Task<(bool success, string message)> DarDeBajaCourt(int id)
        {
            var court = await _context.Courts
                .Include(c => c.disponibilities)
                .Include(c => c.reservations)
                .FirstOrDefaultAsync(c => c.idCourt == id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.isAvailable = false;

            foreach (var disp in court.disponibilities)
                disp.isAvailable = false;

            double totalReembolsos = 0;

            foreach (var reserva in court.reservations)
            {
                if (reserva.isPaid)
                    totalReembolsos += reserva.totalPrice;

                _context.Reservations.Remove(reserva);
            }

            await _context.SaveChangesAsync();

            return (
                true,
                $"Cancha dada de baja exitosamente. " +
                $"Total de reembolsos a procesar: {totalReembolsos:C}"
            );
        }

        // BAJA FÍSICA
        public async Task<(bool success, string message)> EliminarCourtFisico(int id)
        {
            var court = await _context.Courts
                .Include(c => c.disponibilities)
                .Include(c => c.reservations)
                .FirstOrDefaultAsync(c => c.idCourt == id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            double totalReembolsos = 0;

            foreach (var reserva in court.reservations)
            {
                if (reserva.isPaid)
                    totalReembolsos += reserva.totalPrice;
            }

            _context.Disponibilities.RemoveRange(court.disponibilities);
            _context.Reservations.RemoveRange(court.reservations);
            _context.Courts.Remove(court);

            await _context.SaveChangesAsync();

            return (
                true,
                $"Cancha eliminada físicamente. " +
                $"Total de reembolsos a procesar: {totalReembolsos:C}"
            );
        }

        public async Task<CourtDetailDTO?> ConsultarCourt(int id)
        {
            return await _context.Courts
                .Include(c => c.courtType)
                .Include(c => c.disponibilities)
                .Where(c => c.idCourt == id)
                .Select(c => new CourtDetailDTO
                {
                    idCourt = c.idCourt,
                    name = c.name,
                    isAvailable = c.isAvailable,
                    description = c.description,
                    courtTypeName = c.courtType.name,
                    disponibilities = c.disponibilities.Select(d => new DisponibilitySummaryDTO
                    {
                        idDisponibility = d.idDisponibility,
                        day = d.day,
                        startTime = d.startTime,
                        endTime = d.endTime,
                        isAvailable = d.isAvailable
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool success, string message)> HabilitarCourt(int id)
        {
            var court = await _context.Courts.FindAsync(id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.isAvailable = true;

            await _context.SaveChangesAsync();

            return (true, "Cancha habilitada exitosamente.");
        }

        public async Task<(bool success, string message)> DeshabilitarCourt(int id)
        {
            var court = await _context.Courts.FindAsync(id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.isAvailable = false;

            var reservas = await _context.Reservations
                .Where(r => r.idCourt == id)
                .ToListAsync();

            double totalReembolsos = 0;

            foreach (var reserva in reservas)
            {
                if (reserva.isPaid)
                    totalReembolsos += reserva.totalPrice;

                _context.Reservations.Remove(reserva);
            }

            await _context.SaveChangesAsync();

            return (
                true,
                $"Cancha deshabilitada exitosamente. " +
                $"Total de reembolsos a procesar: {totalReembolsos:C}"
            );
        }

        public async Task<(bool success, string message)> ConsultarDisponibilidadCancha(
            int id,
            DateTime fecha,
            TimeSpan hora)
        {
            var court = await _context.Courts.FindAsync(id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            var disponible = await EsDisponible(id, fecha, hora);

            return disponible
                ? (true, "La cancha esta disponible en ese horario.")
                : (false, "La cancha no esta disponible en ese horario.");
        }
    }
}
