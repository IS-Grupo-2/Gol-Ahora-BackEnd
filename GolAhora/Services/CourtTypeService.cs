// CourtTypeService.cs
using GolAhora.DTOs;
using GolAhora.Models;

namespace GolAhora.Services
{
    public class CourtTypeService
    {
        private readonly GolAhora.Data.AppContext _context;

        public CourtTypeService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF11 – Registrar tipo de cancha
        public async Task<(bool success, string message)> AgregarCourtType(CourtTypeDTO dto)
        {
            var courtType = new CourtType
            {
                name = dto.name,
                superficie = dto.superficie,
                capacity = dto.capacity,
                pricePerHour = dto.pricePerHour,
                description = dto.description
            };

            _context.CourtTypes.Add(courtType);
            await _context.SaveChangesAsync();
            return (true, "Tipo de cancha registrado exitosamente.");
        }

        // RF12 – Modificar tipo de cancha
        public async Task<(bool success, string message)> ModificarCourtType(int id, CourtTypeDTO dto)
        {
            var courtType = await _context.CourtTypes.FindAsync(id);
            if (courtType == null)
                return (false, "Tipo de cancha no encontrado.");

            courtType.name = dto.name;
            courtType.superficie = dto.superficie;
            courtType.capacity = dto.capacity;
            courtType.pricePerHour = dto.pricePerHour;
            courtType.description = dto.description;

            await _context.SaveChangesAsync();
            return (true, "Tipo de cancha modificado exitosamente.");
        }
    }
}