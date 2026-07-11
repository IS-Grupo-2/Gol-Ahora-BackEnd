using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GolAhora.Services;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly Services.TeamService _teamService;

        public TeamController(Services.TeamService teamService)
        {
            _teamService = teamService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] TeamDTO teamDto)
        {
            var mensaje = await _teamService.CreateTeam(teamDto);
            if (mensaje != "Equipo creado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }

        [HttpPut("{idTeam}")]
        public async Task<IActionResult> UpdateTeam(int idTeam, [FromBody] TeamDTO teamDto) {
            var mensaje = await _teamService.UpdateTeam(idTeam, teamDto);
            if (mensaje != "Equipo actualizado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }

        [HttpGet("{idTeam}")]
        public async Task<IActionResult> GetTeamById(int idTeam)
        {
            var (team, mensaje) = await _teamService.GetTeamById(idTeam);
            if (mensaje != "Equipo encontrado exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(team);
        }

        [HttpDelete("{idTeam}")]
        public async Task<IActionResult> DeleteTeam(int idTeam)
        {
            var mensaje = await _teamService.DeleteTeam(idTeam);
            if (mensaje != "Equipo eliminado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeams()
        {
            var (teams, mensaje) = await _teamService.GetAllTeams();
            if (mensaje != "Equipos encontrados exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(teams);
        }

        [HttpPut("{idTeam}/addMember/{idPlayer}")]
        public async Task<IActionResult> AddMember(int idTeam, int idPlayer)
        {
            var mensaje = await _teamService.AddMember(idTeam, idPlayer);
            if (mensaje != "Miembro agregado exitosamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }

        [HttpPut("{idTeam}/delegateCaptain/{idPlayer}")]
        public async Task<IActionResult> DelegateACaptain(int idTeam, int idPlayer)
        {
            var mensaje = await _teamService.DelegateACaptain(idTeam, idPlayer);
            if (mensaje != "Capitan asignado correctamente")
            {
                return BadRequest(new { message = mensaje });
            }
            return Ok(mensaje);
        }
        [HttpGet("/api/equipos")]
        public async Task<IActionResult> GetEquiposApiContract() => ToActionResult(await _teamService.GetEquiposApiContract());

        [HttpPost("/api/equipos")]
        public async Task<IActionResult> CreateEquipoApiContract([FromBody] EquipoApiRequest body) => ToActionResult(await _teamService.CreateEquipoApiContract(body));

        [HttpPut("/api/equipos/{id}")]
        public async Task<IActionResult> UpdateEquipoApiContract(int id, [FromBody] EquipoApiRequest body) => ToActionResult(await _teamService.UpdateEquipoApiContract(id, body));

        [HttpDelete("/api/equipos/{id}")]
        public async Task<IActionResult> DeleteEquipoApiContract(int id) => ToActionResult(await _teamService.DeleteEquipoApiContract(id));

        private IActionResult ToActionResult(ApiResult<object> result)
        {
            if (result.Success) return Ok(result.Data);
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
    
}








