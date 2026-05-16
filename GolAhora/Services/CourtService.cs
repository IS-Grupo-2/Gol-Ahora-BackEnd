// CourtService.cs
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

        // RF13 – Listar todas las canchas
        public async Task<List<Court>> ListarCourts()
        {
            return await _context.Courts
                .Include(c => c.courtType)
                .Include(c => c.disponibilities)
                .ToListAsync();
        }

        // RF14 – Baja lógica de cancha y sus disponibilidades
        public async Task<(bool success, string message)> DarDeBajaCourt(int id)
        {
            var court = await _context.Courts
                .Include(c => c.disponibilities)
                .FirstOrDefaultAsync(c => c.idCourt == id);

            if (court == null)
                return (false, "Cancha no encontrada.");

            court.isAvailable = false;

            foreach (var disponibility in court.disponibilities)
                disponibility.isAvailable = false;

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

        // RF18 – Habilitar disponibilidad de una cancha
        public async Task<(bool success, string message)> HabilitarDisponibility(int id)
        {
            var disponibility = await _context.Disponibilities.FindAsync(id);
            if (disponibility == null)
                return (false, "Disponibilidad no encontrada.");

            disponibility.isAvailable = true;
            await _context.SaveChangesAsync();
            return (true, "Disponibilidad habilitada exitosamente.");
        }
    }
}