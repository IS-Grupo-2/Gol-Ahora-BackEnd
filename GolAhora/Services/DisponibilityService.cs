using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class DisponibilityService
    {
        private readonly GolAhora.Data.AppContext _context;

        public DisponibilityService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // verificar(fecha, hora) – verifica disponibilidad en fecha y hora especifica
        private async Task<bool> Verificar(int courtId, DateTime fecha, TimeSpan hora)
        {
            return await _context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == fecha.DayOfWeek &&
                d.startTime <= hora &&
                d.endTime >= hora &&
                d.isAvailable
            );
        }

        // RF16 – Registrar disponibilidad de una cancha
        public async Task<(bool success, string message)> AgregarDisponibility(DisponibilityDTO dto)
        {
            // Validar que no haya superposición de horarios para la misma cancha y día
            var superpuesta = await _context.Disponibilities.AnyAsync(d =>
                d.courtId == dto.courtId &&
                d.day == dto.day &&
                d.isAvailable &&
                d.startTime < dto.endTime &&
                d.endTime > dto.startTime
            );

            if (superpuesta)
                return (false, "Ya existe una disponibilidad activa que se superpone en ese horario.");

            var disponibility = new Disponibility
            {
                day = dto.day,
                startTime = dto.startTime,
                endTime = dto.endTime,
                isAvailable = dto.isAvailable,
                courtId = dto.courtId
            };

            _context.Disponibilities.Add(disponibility);
            await _context.SaveChangesAsync();
            return (true, "Disponibilidad registrada exitosamente.");
        }

        // Modificar disponibilidad
        public async Task<(bool success, string message)> ModificarDisponibility(int id, DisponibilityDTO dto)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.day = dto.day;
            disponibility.startTime = dto.startTime;
            disponibility.endTime = dto.endTime;
            disponibility.isAvailable = dto.isAvailable;
            disponibility.courtId = dto.courtId;

            await _context.SaveChangesAsync();
            return (true, "Disponibilidad modificada exitosamente.");
        }

        // RF17 – Consultar disponibilidad por ID
        public async Task<Disponibility?> ConsultarDisponibility(int id)
        {
            return await _context.Disponibilities
                .Include(d => d.court)
                .FirstOrDefaultAsync(d => d.idDisponibility == id);
        }

        // Listar todas las disponibilidades
        public async Task<List<Disponibility>> ListarDisponibilities()
        {
            return await _context.Disponibilities
                .Include(d => d.court)
                .ToListAsync();
        }

        // RF18 – Habilitar disponibilidad
        public async Task<(bool success, string message)> HabilitarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.isAvailable = true;
            await _context.SaveChangesAsync();
            return (true, "Disponibilidad habilitada exitosamente.");
        }

        // RF18 – Deshabilitar disponibilidad
        public async Task<(bool success, string message)> DeshabilitarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.isAvailable = false;
            await _context.SaveChangesAsync();
            return (true, "Disponibilidad deshabilitada exitosamente.");
        }

        // Consultar disponibilidad en fecha y hora especifica
        public async Task<(bool success, string message)> ConsultarDisponibilidadEnHorario(int courtId, DateTime fecha, TimeSpan hora)
        {
            var disponible = await Verificar(courtId, fecha, hora);
            return disponible
                ? (true, "La cancha está disponible en ese horario.")
                : (false, "La cancha no está disponible en ese horario.");
        }
    }
}
