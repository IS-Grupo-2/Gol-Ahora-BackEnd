using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class LeagueService
    {
        private readonly GolAhora.Data.AppContext _context;

        public LeagueService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }
        //RF 27 - CREAR LIGA
        public async Task<string?> CreateLeague(LeagueDto leagueDto)
        {
            if (leagueDto == null)
            {
                return "Datos invalidos";
            }
            if (string.IsNullOrEmpty(leagueDto.Name))
            {
                return "El nombre de la liga es obligatorio.";
            }
            if (string.IsNullOrEmpty(leagueDto.Description))
            {
                return "La descripción de la liga es obligatoria.";
            }
            if (leagueDto.StartDate >= leagueDto.EndDate)
            {
                return "La fecha de inicio debe ser anterior a la fecha de fin.";
            }
            if (leagueDto.CapacityTeams <= 0)
            {
                return "La capacidad de equipos debe ser mayor a cero.";
            }

            var ligas = await _context.Leagues.ToListAsync();
            int contador = ligas.Count() + 1;

            var liga = new League
            {
                idLeague = contador, // deberia ser generado automáticamente por la base de datos
                name = leagueDto.Name,
                description = leagueDto.Description,
                startDate = leagueDto.StartDate,
                endDate = leagueDto.EndDate,
                isActive = true,
                regulations = "Reglas de la AFA",
                capacityTeams = leagueDto.CapacityTeams
            };
            await _context.Leagues.AddAsync(liga);
            await _context.SaveChangesAsync();
            return "liga creada exitosamente";
        }
        //RF 28 - ACTUALIZAR LIGA
        public async Task<string?> UpdateLeague(int idLeague, LeagueDto leagueDto)
        {
            if (leagueDto == null)
            {
                return "Datos invalidos";
            }
            var liga = await _context.Leagues.FindAsync(idLeague);
            if (liga == null)
            {
                return "Liga no encontrada";
            }
            if (string.IsNullOrEmpty(leagueDto.Name))
            {
                return "El nombre de la liga es obligatorio.";
            }
            if (string.IsNullOrEmpty(leagueDto.Description))
            {
                return "La descripción de la liga es obligatoria.";
            }
            if (leagueDto.StartDate >= leagueDto.EndDate)
            {
                return "La fecha de inicio debe ser anterior a la fecha de fin.";
            }
            if (leagueDto.CapacityTeams <= 0)
            {
                return "La capacidad de equipos debe ser mayor a cero.";
            }
            liga.name = leagueDto.Name;
            liga.description = leagueDto.Description;
            liga.startDate = leagueDto.StartDate;
            liga.endDate = leagueDto.EndDate;
            liga.capacityTeams = leagueDto.CapacityTeams;
            _context.Leagues.Update(liga);
            await _context.SaveChangesAsync();
            return "liga actualizada exitosamente";
        }
        //RF 30 - ELIMINAR LIGA
        public async Task<string?> DeleteLeague(int idLeague)
        {
            var liga = await _context.Leagues.FindAsync(idLeague);
            if (liga == null)
            {
                return "Liga no encontrada";
            }
            liga.isActive = false;
            _context.Leagues.Update(liga);
            await _context.SaveChangesAsync();
            return "liga eliminada exitosamente";
        }
        //RF 31 - CONSULTAR LIGA POR ID PARA IMPRIMIRLA EN LA LANDING PAGE
        public async Task<(LeagueDto liga, string message)> GetLeagueById(int idLeague)
        {
            var liga = await _context.Leagues.FindAsync(idLeague);
            if (liga == null)
            {
                return (new LeagueDto(), "No se encontraron ligas"); ;
            }
            var leagueDto = new LeagueDto
            {
                IdLeague = liga.idLeague,
                Name = liga.name,
                Description = liga.description,
                StartDate = liga.startDate,
                EndDate = liga.endDate,
                IsActive = liga.isActive,
                Regulations = liga.regulations,
                CapacityTeams = liga.capacityTeams
            };
            return (leagueDto, "liga encontrada exitosamente");
        }
        //RF 29 - CONSULTAR TODAS LAS LIGAS PARA IMPRIMIRLAS EN LA LANDING PAGE
        public async Task<(List<LeagueDto> ligas, string message)> GetAllLeagues()
        {
            
            var ligas = await _context.Leagues.ToListAsync();
            if (ligas == null || ligas.Count == 0)
            {
                return (new List<LeagueDto>(), "No se encontraron ligas");
            }

            return (ligas.Select(liga => new LeagueDto
            {
                IdLeague = liga.idLeague,
                Name = liga.name,
                Description = liga.description,
                StartDate = liga.startDate,
                EndDate = liga.endDate,
                IsActive = liga.isActive,
                Regulations = liga.regulations,
                CapacityTeams = liga.capacityTeams
            }).ToList(), "ligas encontradas exitosamente");
        }
        //RF 32 - INCRIBIR EQUIPO A LIGA
        //public async Task<string> incribirEquipo(TeamDTO equipoDto, int idLeague)
        //{
        //    var liga = await _context.Leagues.FindAsync(idLeague);
        //    if (liga == null)
        //    {
        //        return "Liga no encontrada";
        //    }
        //    liga.teams.Add(equipoDto);
        //    _context.Leagues.Update(liga);
        //    await _context.SaveChangesAsync();
        //    return "Equipo inscripto exitosamente";
        //}
    }
}
