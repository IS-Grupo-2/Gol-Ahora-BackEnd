// DisponibilityService.cs
using GolAhora.DTOs;
using GolAhora.Models;

namespace GolAhora.Services
{
    public class DisponibilityService
    {
        private readonly GolAhora.Data.AppContext _context;

        public DisponibilityService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF16 – Registrar disponibilidad de una cancha
        public async Task<(bool success, string message)> AgregarDisponibility(DisponibilityDTO dto)
        {
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
    }
}