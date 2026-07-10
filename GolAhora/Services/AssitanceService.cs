using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using GolAhora.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GolAhora.Services
{
    public class AssistanceService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public AssistanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }

        // + registrar(): void (RF36)
        public async Task<(bool success, string message)> RegistrarAsistencia(AssistanceDTO dto)
        {
            var nuevaAsistencia = new Assistance
            {
                clientId = dto.idClient,
                classId = dto.classId,
                isAssisted = dto.isAssisted,
                date = DateTime.Now,
                observations = dto.observations
            };

            _context.Assistances.Add(nuevaAsistencia);

            await _unitOfWork.SaveChangesAsync();

            return (true, "Asistencia registrada exitosamente.");
        }


        // + modificar(): void
        public async Task<(bool success, string message)> ModificarAsistencia(int idAssistance, bool nuevoEstado, string nuevasObservaciones)
        {
            var asistencia = await _context.Set<Assistance>().FindAsync(idAssistance);
            if (asistencia == null)
                return (false, "Registro de asistencia no encontrado.");

            asistencia.isAssisted = nuevoEstado;
            asistencia.observations = nuevasObservaciones;

            await _unitOfWork.SaveChangesAsync();
            return (true, "Asistencia modificada con éxito.");
        }

        // + consultar(): Asistencia
        public async Task<Assistance?> ConsultarAsistencia(int idAssistance)
        {
            return await _context.Set<Assistance>().FindAsync(idAssistance);
        }

        // + listar(): List<Asistencia>
        public async Task<List<Assistance>> ListarAsistencias()
        {
            return await _context.Set<Assistance>().ToListAsync();
        }
    }
}


