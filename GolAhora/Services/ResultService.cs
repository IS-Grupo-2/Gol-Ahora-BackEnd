using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ResultService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public ResultService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }
        //NO SE SI HACER UNA MODIFICACION DEL RESULTADO ACA O EN EL RESULT SERVICE,
        //SI LO HAGO ACA, DEBERIA RECIBIR UN MATCHDTO CON LOS DATOS DEL PARTIDO Y LOS RESULTADOS ACTUALIZADOS, SI LO HAGO EN EL RESULT SERVICE, SOLO RECIBO 
        //UN RESULTDTO CON LOS DATOS DEL RESULTADO A ACTUALIZAR Y EL ID DEL PARTIDO AL QUE PERTENECE, POR AHORA LO DEJO EN EL RESULT SERVICE

        public async Task<string> UpdateResult(int matchId, ResultDTO resultDto)
        {
            if (resultDto == null) {
                return "El resultado proporcionado es nulo";
            }
            if(resultDto.scoreTeamLocal < 0 || resultDto.scoreTeamVisitor < 0) {
                return "Los goles no pueden ser negativos";
            }
            var result = await _context.Results.FirstOrDefaultAsync(r => r.idMatch == matchId);
            if (result == null) {
                return "No se encontró el resultado para el partido especificado";
            }
            result.scoreTeamLocal = resultDto.scoreTeamLocal;
            result.scoreTeamVisitor = resultDto.scoreTeamVisitor;

            await _unitOfWork.SaveChangesAsync();

            return "Resultado actualizado exitosamente";
        }

        public async Task<(List<ResultDTO> resultDTOs, string mensaje)> GetAllResults()
        {
            var listaderesultados = await _context.Results.ToListAsync();
            if (listaderesultados == null || listaderesultados.Count() == 0)
            {
                return (new List<ResultDTO>(), "No se encontraron resultados");
            }
            return (listaderesultados.Select(x => new ResultDTO
            {
                idResults = x.idResults,
                idMatch = x.idMatch,
                scoreTeamLocal = x.scoreTeamLocal,
                scoreTeamVisitor = x.scoreTeamVisitor,
            }).ToList(), "Resultados encontrados existosamente");
        }

        public async Task<(ResultDTO result, string mensaje)> GetResultById(int matchId)
        {
            var result = await _context.Results.FirstOrDefaultAsync(r => r.idMatch == matchId);
            if (result == null)
            {
                return (new ResultDTO(), "No se encontró el resultado para el partido especificado");
            }
            return (new ResultDTO
            {
                idResults = result.idResults,
                idMatch = result.idMatch,
                scoreTeamLocal = result.scoreTeamLocal,
                scoreTeamVisitor = result.scoreTeamVisitor
            }, "Resultado encontrado exitosamente");
        }
    }
}



