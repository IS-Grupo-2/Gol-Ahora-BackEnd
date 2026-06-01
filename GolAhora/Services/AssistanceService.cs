using GolAhora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolAhora.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class AssistanceService
    {
        private readonly GolAhora.Data.AppContext _context;

        public AssistanceService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> RegistrarAsistencia(AssistanceDTO dto)// RF40 – Registrar la asistencia
        {
            var nuevaAsistencia = new Assistance
            {
                clientId = dto.clienteId,
                classId = dto.classId,
                isAssisted = dto.isAssisted,
                observations = dto.observations
            };
            
            _context.Assistances.Add(nuevaAsistencia); // Agregalo a la tabla Assistances

            await _context.SaveChangesAsync();// 4. Impactamos los cambios físicamente en tu SQL Server local

            return (true, "Asistencia registrada exitosamente.");
        }


        // RF41 – Modificar la asistencia
        public async Task<(bool success, string message)> ModificarAsistencia(int id, AssistanceDTO dto)
        {
            // asistencia en la base de datos por su ID
            var asistencia = await _context.Assistances.FindAsync(id);
            if (asistencia == null)
                return (false, "Registro de asistencia no encontrado.");

            // Modificacion de lso datos
            asistencia.clientId = dto.clienteId;
            asistencia.classId = dto.classId;
            asistencia.isAssisted = dto.isAssisted;
            asistencia.observations = dto.observations;

            await _context.SaveChangesAsync();
            return (true, "Asistencia modificada exitosamente.");

            
        }

        // RF42 – Consultar asistencia por ID
        public async Task<AssistanceDetailDTO?> ConsultarAsistencia(int id)
        {
            return await _context.Assistances
                .Where(a => a.idAssistance == id)
                .Select(a => new AssistanceDetailDTO
                {
                    idAssistance = a.idAssistance,
                    clienteId = a.clientId,
                    classId = a.classId,
                    isAssisted = a.isAssisted,
                    observations = a.observations
                })
                .FirstOrDefaultAsync();
        }

        // RF43 – Listar las asistencias
        public async Task<List<AssistanceResponseDTO>> ListarAsistencias()
        {
            return await _context.Assistances
                .Select(a => new AssistanceResponseDTO
                {
                    idAssistance = a.idAssistance,
                    clienteId = a.clientId,
                    classId = a.classId,
                    isAssisted = a.isAssisted,
                    observations = a.observations
                })
                .ToListAsync();
        }
    }
}
