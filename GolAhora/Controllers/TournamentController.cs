using GolAhora.Services;
using GolAhora.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly TournamentService _torneoService;
        private readonly MatchService _matchService;

        public TournamentController(TournamentService torneoService, MatchService matchService)
        {
            _torneoService = torneoService;
            _matchService = matchService;
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
        [HttpGet("/api/competencias")]
        public async Task<IActionResult> GetCompetenciasApiContract() => ToActionResult(await _torneoService.GetCompetenciasApiContract());

        [HttpPost("/api/competencias")]
        public async Task<IActionResult> CreateCompetenciaApiContract([FromBody] CompetenciaRequest body) => ToActionResult(await _torneoService.CreateCompetenciaApiContract(body));

        [HttpPut("/api/competencias/{id}")]
        public async Task<IActionResult> UpdateCompetenciaApiContract(int id, [FromBody] CompetenciaRequest body) => ToActionResult(await _torneoService.UpdateCompetenciaApiContract(id, body));

        [HttpDelete("/api/competencias/{id}")]
        public async Task<IActionResult> DeleteCompetenciaApiContract(int id) => ToActionResult(await _torneoService.DeleteCompetenciaApiContract(id));

        [HttpPost("/api/competencias/{competenciaId}/inscribir")]
        public async Task<IActionResult> InscribirEquipoApiContract(int competenciaId, [FromBody] InscripcionEquipoRequest body) => ToActionResult(await _torneoService.InscribirEquipoApiContract(competenciaId, body));

        [HttpPost("/api/competencias/{competenciaId}/fixture")]
        public async Task<IActionResult> GenerarFixtureApiContract(int competenciaId) => ToActionResult(await _matchService.GenerarFixtureApiContract(competenciaId));

        private IActionResult ToActionResult(ApiResult<object> result)
        {
            if (result.Success) return Ok(result.Data);
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}





