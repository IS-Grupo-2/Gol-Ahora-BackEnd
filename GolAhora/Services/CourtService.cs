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

        // disponible(fecha, hora) – verifica si la cancha esta disponible
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

        // Registrar una nueva cancha
        public async Task<(bool success, string message)> AgregarCourt(CourtDTO dto)
        {
            var courtType = await _context.CourtTypes.FindAsync(dto.courtTypeId);
            if (courtType == null)
                return (false, "El tipo de cancha no existe.");

            var court = new Court
            {
                name = dto.name,
                description = dto.description,
                imageUrl = dto.imageUrl,
                courtTypeId = dto.courtTypeId,
                isAvailable = true
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();
            return (true, "Cancha registrada exitosamente.");
        }

        // Modificar cancha
        public async Task<(bool success, string message)> ModificarCourt(int id, CourtDTO dto)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null)
                return (false, "Cancha no encontrada.");

            court.name = dto.name;
            court.description = dto.description;
            court.imageUrl = dto.imageUrl;
            court.courtTypeId = dto.courtTypeId;

            await _context.SaveChangesAsync();
            return (true, "Cancha modificada exitosamente.");
        }

        // RF13 – Listar todas las canchas
        public async Task<List<Court>> ListarCourts()
        {
            return await _context.Courts
                .Include(c => c.courtType)
                .Include(c => c.disponibilities)
                .ToListAsync();
        }

        // RF14 – Baja logica de cancha y sus disponibilidades
        public async Task<(bool success, string message)> DarDeBajaCourt(int id)
        {
            var court = await _context.Courts
                .Include(c => c.disponibilities)
                .FirstOrDefaultAsync(c => c.idCourt == id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.isAvailable = false;

            foreach (var disp in court.disponibilities)
                disp.isAvailable = false;

            await _context.SaveChangesAsync();
            return (true, "Cancha dada de baja exitosamente.");
        }

        // RF15 – Consultar cancha por ID
        public async Task<Court?> ConsultarCourt(int id)
        {
            return await _context.Courts
                .Include(c => c.courtType)
                .Include(c => c.disponibilities)
                .FirstOrDefaultAsync(c => c.idCourt == id);
        }

        // disponible(fecha, hora) – consultar si cancha esta disponible
        public async Task<(bool success, string message)> ConsultarDisponibilidadCancha(int id, DateTime fecha, TimeSpan hora)
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
