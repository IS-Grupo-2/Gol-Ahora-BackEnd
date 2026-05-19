using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

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

        // RF13 – Listar todos los tipos de cancha
        public async Task<List<CourtType>> ListarCourtTypes()
        {
            return await _context.CourtTypes
                .Include(ct => ct.courts)
                .ToListAsync();
        }

        // RF15 – Consultar tipo de cancha por ID
        public async Task<CourtType?> ConsultarCourtType(int id)
        {
            return await _context.CourtTypes
                .Include(ct => ct.courts)
                .FirstOrDefaultAsync(ct => ct.idTypeCourt == id);
        }

        // RF14 – Baja lógica que deshabilita todas las canchas y disponibilidades del tipo de cancha
        public async Task<(bool success, string message)> EliminarCourtType(int id)
        {
            var courtType = await _context.CourtTypes
                .Include(ct => ct.courts)
                .ThenInclude(c => c.disponibilities)
                .FirstOrDefaultAsync(ct => ct.idTypeCourt == id);

            if (courtType == null)
                return (false, "Tipo de cancha no encontrado.");

            foreach (var court in courtType.courts)
            {
                court.isAvailable = false;
                foreach (var disp in court.disponibilities)
                    disp.isAvailable = false;
            }

            await _context.SaveChangesAsync();
            return (true, "Tipo de cancha dado de baja exitosamente.");
        }

        // GenerarReporte de tipos de cancha
        public async Task<object> GenerarReporte()
        {
            var courtTypes = await _context.CourtTypes
                .Include(ct => ct.courts)
                .ThenInclude(c => c.reservations)
                .ToListAsync();

            var reporte = courtTypes.Select(ct => new
            {
                idTipoCancha = ct.idTypeCourt,
                nombre = ct.name,
                superficie = ct.superficie,
                capacidad = ct.capacity,
                precioPorHora = ct.pricePerHour,
                totalCanchas = ct.courts.Count,
                canchasActivas = ct.courts.Count(c => c.isAvailable),
                totalReservas = ct.courts.Sum(c => c.reservations.Count),
                ingresosTotales = ct.courts
                                    .SelectMany(c => c.reservations)
                                    .Sum(r => r.totalPrice)
            });

            return new
            {
                fechaGeneracion = DateTime.Now,
                tiposDeCanchas = reporte
            };
        }
    }
}