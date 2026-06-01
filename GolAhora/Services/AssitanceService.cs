using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GolAhora.Services
{
    public class AssistanceService
    {
        private readonly GolAhora.Data.AppContext _context;

        public AssistanceService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // + registrar(): void (RF36)
        public async Task<(bool success, string message)> RegistrarAsistenciasClase(int idClass, List<AssistanceDTO> dtos)
        {
            var clase = await _context.Classes.FindAsync(idClass);
            if (clase == null)
                return (false, "La clase especificada no existe.");

            // Eliminamos registros previos si el profesor re-envía la lista para evitar duplicados
            var previas = await _context.Set<Assistance>().Where(a => a.classId == idClass).ToListAsync();
            if (previas.Any())
            {
                _context.Set<Assistance>().RemoveRange(previas);
            }

            foreach (var dto in dtos)
            {
                var nuevaAsistencia = new Assistance
                {
                    classId = idClass,
                    clientId = dto.idClient,
                    date = DateTime.Now,
                    isAssisted = dto.isAssisted,
                    observations = dto.observations
                };
                _context.Set<Assistance>().Add(nuevaAsistencia);
            }

            await _context.SaveChangesAsync();
            return (true, $"Se registraron {dtos.Count} asistencias correctamente.");
        }

        // + modificar(): void
        public async Task<(bool success, string message)> ModificarAsistencia(int idAssistance, bool nuevoEstado, string nuevasObservaciones)
        {
            var asistencia = await _context.Set<Assistance>().FindAsync(idAssistance);
            if (asistencia == null)
                return (false, "Registro de asistencia no encontrado.");

            asistencia.isAssisted = nuevoEstado;
            asistencia.observations = nuevasObservaciones;

            await _context.SaveChangesAsync();
            return (true, "Asistencia modificada con éxito.");
        }
    }
}