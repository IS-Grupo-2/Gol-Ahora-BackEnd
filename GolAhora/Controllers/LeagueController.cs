using GolAhora.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        private readonly LeagueService _ligasService;

        public LeagueController(LeagueService ligasService)
        {
            _ligasService = ligasService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague([FromBody] GolAhora.DTOs.LeagueDto leagueDto)
        {
            var mensaje = await _ligasService.CreateLeague(leagueDto);
            if (mensaje != "liga creada exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "liga creada exitosamente" });
        }

        [HttpPut("{idLeague}")]
        public async Task<IActionResult> UpdateLeague(int idLeague, [FromBody] GolAhora.DTOs.LeagueDto leagueDto)
        {
            var mensaje = await _ligasService.UpdateLeague(idLeague, leagueDto);
            if (mensaje != "liga actualizada exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "liga actualizada exitosamente" });
        }

        [HttpDelete("{idLeague}")]
        public async Task<IActionResult> DeleteLeague(int idLeague)
        {
            var mensaje = await _ligasService.DeleteLeague(idLeague);
            if (mensaje != "liga eliminada exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(new { message = "liga eliminada exitosamente" });
        }

        [HttpGet("{idLeague}")]
        public async Task<IActionResult> GetLeagueById(int idLeague)
        {
            var (liga, mensaje) = await _ligasService.GetLeagueById(idLeague);
            if (mensaje != "liga encontrada exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(liga);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllLeagues()
        {
            var (ligas, mensaje) = await _ligasService.GetAllLeagues();
            if (mensaje != "ligas encontradas exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(ligas);
        }
    }
}
