using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GolAhora.Services;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly MatchService _matchService;
        public MatchController(MatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] MatchDTO matchDto)
        {
            var mensaje = await _matchService.CreateMatch(matchDto);
            if (mensaje != "Partido creado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMatchById(int idMatch)
        {
            var (partido, mensaje) = await _matchService.GetMatchById(idMatch);
            if (mensaje != "Partido encontrado exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(partido);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMatches()
        {
            var (matches, mensaje) = await _matchService.GetAllMatches();
            if (mensaje != "Partidos encontrados exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(matches);
        }
        [HttpGet("/api/fixtures")]
        public async Task<IActionResult> GetFixturesApiContract() => ToActionResult(await _matchService.GetFixturesApiContract());

        [HttpPatch("/api/fixtures/{competenciaId}/partido/{partidoId}/resultado")]
        public async Task<IActionResult> RegistrarResultadoApiContract(int competenciaId, int partidoId, [FromBody] ResultadoPartidoRequest body) => ToActionResult(await _matchService.RegistrarResultadoApiContract(competenciaId, partidoId, body));

        private IActionResult ToActionResult(ApiResult<object> result)
        {
            if (result.Success) return Ok(result.Data);
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}








