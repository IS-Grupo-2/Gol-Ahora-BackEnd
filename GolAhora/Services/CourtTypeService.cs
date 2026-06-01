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
            // Validar nombre
            if (string.IsNullOrWhiteSpace(dto.name))
                return (false, "El nombre del tipo de cancha es obligatorio.");

            // Validar superficie
            if (dto.superficie <= 0)
                return (false, "La superficie debe ser mayor a 0.");

            // Validar capacidad
            if (dto.capacity <= 0)
                return (false, "La capacidad debe ser mayor a 0.");

            // Validar precio
            if (dto.pricePerHour <= 0)
                return (false, "El precio por hora debe ser mayor a 0.");

            // Validar tipo duplicado por nombre o por superficie o capacidad
            var existe = await _context.CourtTypes.AnyAsync(ct =>
                ct.name.ToLower() == dto.name.ToLower() ||
                (ct.superficie == dto.superficie && ct.capacity == dto.capacity)
            );

            if (existe)
                return (false, "Ya existe un tipo de cancha con ese nombre o con la misma superficie y capacidad.");

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

            // Validar nombre
            if (string.IsNullOrWhiteSpace(dto.name))
                return (false, "El nombre del tipo de cancha es obligatorio.");

            // Validar superficie
            if (dto.superficie <= 0)
                return (false, "La superficie debe ser mayor a 0.");

            // Validar capacidad
            if (dto.capacity <= 0)
                return (false, "La capacidad debe ser mayor a 0.");

            // Validar precio
            if (dto.pricePerHour <= 0)
                return (false, "El precio por hora debe ser mayor a 0.");

            // Validar tipo duplicado por nombre o por superficie+capacidad, excluyendo el propio registro
            var existe = await _context.CourtTypes.AnyAsync(ct =>
                ct.idTypeCourt != id &&
                (ct.name.ToLower() == dto.name.ToLower() ||
                (ct.superficie == dto.superficie && ct.capacity == dto.capacity))
            );

            if (existe)
                return (false, "Ya existe un tipo de cancha con ese nombre o con la misma superficie y capacidad.");

            courtType.name = dto.name;
            courtType.superficie = dto.superficie;
            courtType.capacity = dto.capacity;
            courtType.pricePerHour = dto.pricePerHour;
            courtType.description = dto.description;

            await _context.SaveChangesAsync();

            return (true, "Tipo de cancha modificado exitosamente.");
        }

        // RF13 – Listar todos los tipos de cancha
        public async Task<List<CourtTypeResponseDTO>> ListarCourtTypes()
        {
            return await _context.CourtTypes
                .Select(ct => new CourtTypeResponseDTO
                {
                    idTypeCourt = ct.idTypeCourt,
                    name = ct.name,
                    superficie = ct.superficie,
                    capacity = ct.capacity,
                    pricePerHour = ct.pricePerHour,
                    description = ct.description
                })
                .ToListAsync();
        }

        // RF15 – Consultar tipo de cancha por ID
        public async Task<CourtTypeDetailDTO?> ConsultarCourtType(int id)
        {
            return await _context.CourtTypes
                .Include(ct => ct.courts)
                .Where(ct => ct.idTypeCourt == id)
                .Select(ct => new CourtTypeDetailDTO
                {
                    idTypeCourt = ct.idTypeCourt,
                    name = ct.name,
                    superficie = ct.superficie,
                    capacity = ct.capacity,
                    pricePerHour = ct.pricePerHour,
                    description = ct.description,
                    courts = ct.courts.Select(c => new CourtSummaryDTO
                    {
                        idCourt = c.idCourt,
                        name = c.name,
                        isAvailable = c.isAvailable
                    }).ToList()
                })
                .FirstOrDefaultAsync();
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
