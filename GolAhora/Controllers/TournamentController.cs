using GolAhora.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly TournamentService _torneoService;

        public TournamentController(TournamentService torneoService)
        {
            _torneoService = torneoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTournament([FromBody] GolAhora.DTOs.TournamentDto tournamentDto)
        {
            var mensaje = await _torneoService.CreateTournament(tournamentDto);
            if (mensaje != "torneo creado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "torneo creado exitosamente" });
        }

        [HttpPut("{idTournament}")]
        public async Task<IActionResult> UpdateTournament(int idTournament, [FromBody] GolAhora.DTOs.TournamentDto tournamentDto)
        {
            var mensaje = await _torneoService.UpdateTournament(idTournament, tournamentDto);
            if (mensaje != "torneo actualizado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "torneo actualizado exitosamente" });
        }

        [HttpDelete("{idTournament}")]
        public async Task<IActionResult> DeleteTournament(int idTournament)
        {
            var mensaje = await _torneoService.DeleteTournament(idTournament);
            if (mensaje != "torneo eliminado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "torneo eliminado exitosamente" });
        }

        [HttpGet("{idTournament}")]
        public async Task<IActionResult> GetTournamentById(int idTournament)
        {
            var (torneo, mensaje) = await _torneoService.GetTournamentById(idTournament);
            if (mensaje != "torneo encontrado exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(torneo);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTournaments()
        {
            var (torneos, mensaje) = await _torneoService.GetAllTournaments();
            if (mensaje != "torneos encontrados exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(torneos);
        }
    }
}
