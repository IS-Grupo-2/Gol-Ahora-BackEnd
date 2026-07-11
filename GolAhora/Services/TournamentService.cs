using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public partial class TournamentService : ServicePayloadBase
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

        protected override GolAhora.Data.AppContext Context => _context;
        protected override IUnitOfWork? UnitOfWork => _unitOfWork;

        public TournamentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }
        //RF 27 - CREAR TORNEO
        public async Task<string?> CreateTournament(TournamentDto tournamentDto)
        {
            
            if (tournamentDto == null)
            {
                return "Datos invalidos";
            }
            if (string.IsNullOrEmpty(tournamentDto.Name))
            {
                return "El nombre del torneo es obligatorio.";
            }
            if (string.IsNullOrEmpty(tournamentDto.Description))
            {
                return "La descripción del torneo es obligatoria.";
            }
            if (tournamentDto.StartDate >= tournamentDto.EndDate)
            {
                return "La fecha de inicio debe ser anterior a la fecha de fin.";
            }
            if (tournamentDto.CapacityTeams <= 0)
            {
                return "La capacidad de equipos debe ser mayor a cero.";
            }
            if (!(tournamentDto.CapacityTeams >= 2 && (tournamentDto.CapacityTeams & (tournamentDto.CapacityTeams-1))==0))
            {
                return "La capacidad debe ser 2, 4, 8, 16, 32";
            }
            var torneos = await _context.Tournaments.ToListAsync();
            int contador = torneos.Count()+1;

            var torneo = new Tournament
            {
                idTournament = contador, // deberia ser generado automáticamente por la base de datos
                name = tournamentDto.Name,
                description = tournamentDto.Description,
                startDate = tournamentDto.StartDate,
                endDate = tournamentDto.EndDate,
                isActive = true,
                regulations = "Reglas de la AFA",
                capacityTeams = tournamentDto.CapacityTeams
            };
            await _context.Tournaments.AddAsync(torneo);
            await _unitOfWork.SaveChangesAsync();
            return "torneo creado exitosamente";
        }
        //RF 28 - ACTUALIZAR TORNEO
        public async Task<string?> UpdateTournament(int idTournament, TournamentDto tournamentDto)
        {
            if (tournamentDto == null)
            {
                return "Datos invalidos";
            }
            var torneo = await _context.Tournaments.FindAsync(idTournament);
            if (torneo == null)
            {
                return "Torneo no encontrado";
            }
            if (string.IsNullOrEmpty(tournamentDto.Name))
            {
                return "El nombre del torneo es obligatorio.";
            }
            if (string.IsNullOrEmpty(tournamentDto.Description))
            {
                return "La descripción del torneo es obligatoria.";
            }
            if (tournamentDto.StartDate >= tournamentDto.EndDate)
            {
                return "La fecha de inicio debe ser anterior a la fecha de fin.";
            }
            if (tournamentDto.CapacityTeams <= 0)
            {
                return "La capacidad de equipos debe ser mayor a cero.";
            }
            torneo.name = tournamentDto.Name;
            torneo.description = tournamentDto.Description;
            torneo.startDate = tournamentDto.StartDate;
            torneo.endDate = tournamentDto.EndDate;
            torneo.capacityTeams = tournamentDto.CapacityTeams;
            _context.Tournaments.Update(torneo);
            await _unitOfWork.SaveChangesAsync();
            return "torneo actualizado exitosamente";
        }
        //RF 30 - ELIMINAR TORNEO
        public async Task<string?> DeleteTournament(int idTournament)
        {
            var torneo = await _context.Tournaments.FindAsync(idTournament);
            if (torneo == null)
            {
                return "Torneo no encontrado";
            }
            torneo.isActive = false;
            _context.Tournaments.Update(torneo);
            await _unitOfWork.SaveChangesAsync();
            return "torneo eliminado exitosamente";
        }
        //RF 31 - CONSULTAR TORNEO POR ID PARA IMPRIMIRLO EN LA LANDING PAGE
        public async Task<(TournamentDto torneo, string message)> GetTournamentById(int idTournament)
        {
            var torneo = await _context.Tournaments.FindAsync(idTournament);
            if (torneo == null)
            {
                return (new TournamentDto(), "No se encontraron torneos"); ;
            }
            var tournamentDto = new TournamentDto
            {
                IdTournament = torneo.idTournament,
                Name = torneo.name,
                Description = torneo.description,
                StartDate = torneo.startDate,
                EndDate = torneo.endDate,
                IsActive = torneo.isActive,
                Regulations = torneo.regulations,
                CapacityTeams = torneo.capacityTeams
            };
            return (tournamentDto, "torneo encontrado exitosamente");
        }
        //RF 29 - CONSULTAR TODAS LOS TORNEOS PARA IMPRIMIRLAS EN LA LANDING PAGE
        public async Task<(List<TournamentDto> torneos, string message)> GetAllTournaments()
        {

            var torneos = await _context.Tournaments.ToListAsync();
            if (torneos == null || torneos.Count == 0)
            {
                return (new List<TournamentDto>(), "No se encontraron torneos");
            }

            return (torneos.Select(torneo => new TournamentDto
            {
                IdTournament = torneo.idTournament,
                Name = torneo.name,
                Description = torneo.description,
                StartDate = torneo.startDate,
                EndDate = torneo.endDate,
                IsActive = torneo.isActive,
                Regulations = torneo.regulations,
                CapacityTeams = torneo.capacityTeams
            }).ToList(), "torneos encontrados exitosamente");
        }
        //RF 32 - INCRIBIR EQUIPO A LIGA
        //public async Task<string> incribirEquipo(EquipoDto equipoDto, int idLeague)
        //{
        //    var liga = await _context.Leagues.FindAsync(idLeague);
        //    if (liga == null)
        //    {
        //        return "Liga no encontrada";
        //    }
        //    liga.teams.Add(equipoDto);
        //    _context.Leagues.Update(liga);
        //    await _unitOfWork.SaveChangesAsync();


        //    return "Equipo inscripto exitosamente";

        //}
    }
}




