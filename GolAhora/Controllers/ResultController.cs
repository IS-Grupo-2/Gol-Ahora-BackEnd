using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly ResultService resultService;

        public ResultController(ResultService resultService)
        {
            this.resultService = resultService;
        }
        [HttpPut("{matchId}")]
        public async Task<IActionResult> UpdateResult(int matchId, ResultDTO resultDto) {
            var mensaje = await resultService.UpdateResult(matchId, resultDto);
            if (mensaje != "Resultado actualizado exitosamente")
            {
                return BadRequest(mensaje);
            }
            return Ok(resultDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResults()
        {
            var (resultDTOs, mensaje) = await resultService.GetAllResults();
            if (mensaje != "Resultados encontrados existosamente")
            {
                return NotFound(mensaje);
            }
            return Ok(resultDTOs);
        }

        [HttpGet("{resultId}")]
        public async Task<IActionResult> GetResultById(int resultId)
        {
            var (result, mensaje) = await resultService.GetResultById(resultId);
            if (mensaje != "Resultado encontrado exitosamente")
            {
                return NotFound(mensaje);
            }
            return Ok(result);
        }

    }
}
