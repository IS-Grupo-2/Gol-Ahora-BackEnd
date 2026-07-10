using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class DisponibilityService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public DisponibilityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }

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

        public async Task<(bool success, string message)> AgregarDisponibility(DisponibilityDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
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
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Disponibilidad registrada exitosamente.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al registrar la disponibilidad: {ex.Message}");
                }
            }
        }

        public async Task<(bool success, string message)> ModificarDisponibility(int id, DisponibilityDTO dto)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            // Validar superposición con otras disponibilidades (excluyendo la actual)
            var superpuesta = await _context.Disponibilities.AnyAsync(d =>
                d.idDisponibility != id &&
                d.courtId == dto.courtId &&
                d.day == dto.day &&
                d.isAvailable &&
                d.startTime < dto.endTime &&
                d.endTime > dto.startTime
            );

            if (superpuesta)
                return (false, "Ya existe una disponibilidad activa que se superpone en ese horario.");

            disponibility.day = dto.day;
            disponibility.startTime = dto.startTime;
            disponibility.endTime = dto.endTime;
            disponibility.isAvailable = dto.isAvailable;
            disponibility.courtId = dto.courtId;

            await _unitOfWork.SaveChangesAsync();
            return (true, "Disponibilidad modificada exitosamente.");
        }

        public async Task<DisponibilityDetailDTO?> ConsultarDisponibility(int id)
        {
            return await _context.Disponibilities
                .Include(d => d.court)
                .Where(d => d.idDisponibility == id)
                .Select(d => new DisponibilityDetailDTO
                {
                    idDisponibility = d.idDisponibility,
                    day = d.day,
                    startTime = d.startTime,
                    endTime = d.endTime,
                    isAvailable = d.isAvailable,
                    courtId = d.courtId,
                    courtName = d.court.name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<DisponibilityResponseDTO>> ListarDisponibilities()
        {
            return await _context.Disponibilities
                .Include(d => d.court)
                .Select(d => new DisponibilityResponseDTO
                {
                    idDisponibility = d.idDisponibility,
                    day = d.day,
                    startTime = d.startTime,
                    endTime = d.endTime,
                    isAvailable = d.isAvailable,
                    courtName = d.court.name
                })
                .ToListAsync();
        }

        public async Task<(bool success, string message)> HabilitarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.isAvailable = true;
            await _unitOfWork.SaveChangesAsync();
            return (true, "Disponibilidad habilitada exitosamente.");
        }

        public async Task<(bool success, string message)> DeshabilitarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.isAvailable = false;
            await _unitOfWork.SaveChangesAsync();
            return (true, "Disponibilidad deshabilitada exitosamente.");
        }

        public async Task<(bool success, string message)> EliminarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            _context.Disponibilities.Remove(disponibility);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Disponibilidad eliminada exitosamente.");
        }

        public async Task<(bool success, string message)> ConsultarDisponibilidadEnHorario(int courtId, DateTime fecha, TimeSpan hora)
        {
            var disponible = await Verificar(courtId, fecha, hora);
            return disponible
                ? (true, "La cancha está disponible en ese horario.")
                : (false, "La cancha no está disponible en ese horario.");
        }
    }
}



