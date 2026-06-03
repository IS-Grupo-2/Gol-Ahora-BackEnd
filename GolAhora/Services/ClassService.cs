using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GolAhora.Services;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ClassService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly AssistanceService _assistanceService;

        public ClassService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF35 + RF37 – Programar clases y entrenamientos grupales configurando el máximo
        public async Task<(bool success, string message, int idClass)> ProgramarClase(ClassDTO dto)
        {
            var cancha = await _context.Courts.FindAsync(dto.courtId);
            if (cancha == null)
                return (false, "La cancha especificada no existe.", 0);

            if (!cancha.isAvailable)
                return (false, "La cancha seleccionada no está disponible.", 0);

            if (!string.IsNullOrWhiteSpace(dto.classType)
                && dto.classType.ToLower().Contains("particular")
                && dto.capacityMax > 2)
                return (false, "Las clases de tipo particular no pueden superar los 2 alumnos.", 0);

            var nuevaClase = new Class
            {
                name = dto.name,
                description = dto.description,
                classType = dto.classType,
                profesorId = dto.profesorId,
                courtId = dto.courtId,
                date = dto.date,
                capacityMax = dto.capacityMax, // RF37
                duration = dto.duration,
                price = dto.price,
                isActive = true
            };

            _context.Classes.Add(nuevaClase);
            await _context.SaveChangesAsync();

            return (true, "Clase programada exitosamente.", nuevaClase.idClass);
        }

        // Modificar clase
        public async Task<(bool success, string message)> ModificarClase(int idClass, ClassDTO dto)
        {
            var clase = await _context.Classes
                .Include(c => c.clients)
                .FirstOrDefaultAsync(c => c.idClass == idClass);

            if (clase == null)
                return (false, "La clase no existe.");

            // No permitir reducir capacidad por debajo de alumnos actuales
            var alumnosActuales = clase.clients?.Count ?? 0;
            if (dto.capacityMax < alumnosActuales)
                return (false, $"La nueva capacidad ({dto.capacityMax}) no puede ser menor que los alumnos actuales ({alumnosActuales}).");

            // Validación negocio particular
            if (!string.IsNullOrWhiteSpace(dto.classType)
                && dto.classType.ToLower().Contains("particular")
                && dto.capacityMax > 2)
                return (false, "Las clases de tipo particular no pueden superar los 2 alumnos.");

            // Actualizar campos (excluyendo id)
            clase.name = dto.name;
            clase.description = dto.description;
            clase.classType = dto.classType;
            clase.profesorId = dto.profesorId;
            clase.courtId = dto.courtId;
            clase.date = dto.date;
            clase.capacityMax = dto.capacityMax;
            clase.duration = dto.duration;
            clase.price = dto.price;

            _context.Classes.Update(clase);
            await _context.SaveChangesAsync();

            return (true, "Clase modificada correctamente.");
        }

        // Cancelar (desactivar) clase
        public async Task<(bool success, string message)> CancelarClase(int idClass)
        {
            var clase = await _context.Classes.FindAsync(idClass);
            if (clase == null)
                return (false, "La clase no existe.");

            if (!clase.isActive)
                return (false, "La clase ya está cancelada.");

            clase.isActive = false;
            _context.Classes.Update(clase);
            await _context.SaveChangesAsync();

            return (true, "Clase cancelada correctamente.");
        }

        // Consultar una clase por id (mapea a DTO)
        public async Task<ClassResponseDTO?> ConsultarClase(int idClass)
        {
            
            return await _context.Classes
                .Where(x => x.idClass == idClass)
                .Select(c => new ClassResponseDTO
                {
                    idClass = c.idClass,
                    name = c.name,
                    description = c.description,
                    classType = c.classType,
                    profesorId = c.profesorId,

                    
                    professorFullName = (c.profesor.personalClubProfile.user.name + " " + c.profesor.personalClubProfile.user.lastName).Trim(),

                    courtId = c.courtId,
                    courtName = c.court.name ?? "",
                    date = c.date,
                    capacityMax = c.capacityMax,
                    currentAlumnosCount = c.clients.Count,
                    duration = c.duration,
                    price = c.price,
                    isActive = c.isActive
                })
                .FirstOrDefaultAsync(); // Si la encuentra devuelve el DTO, si no, devuelve null
        }

        // RF38 – Asignar o cambiar profesor a una clase
        public async Task<(bool success, string message)> AsignarProfesor(int idClass, int profesorId)
        {
            var clase = await _context.Classes.FindAsync(idClass);
            if (clase == null)
                return (false, "La clase o entrenamiento no existe.");

            // RF39: verificación de la certificación deportiva del profesor
            var profesor = await _context.ProfessorProfiles.FindAsync(profesorId);
            if (profesor == null)
                return (false, "El profesor no existe.");

            // Verifica si el profesor tiene certificaciones
            if (profesor.certifications == null || !profesor.certifications.Any())
                return (false, "El profesor no cuenta con la certificación requerida.");

            clase.profesorId = profesorId;
            await _context.SaveChangesAsync();

            return (true, "Profesor asignado correctamente a la clase.");
        }

        public async Task<(bool success, string message)> AgregarAlumno(int idClass, int clientId)
        {
            var clase = await _context.Classes
                .Include(c => c.clients)
                .FirstOrDefaultAsync(c => c.idClass == idClass);

            if (clase == null)
                return (false, "La clase no existe.");

            if (!clase.isActive)
                return (false, "No se pueden agregar alumnos a una clase cancelada.");

            var cliente = await _context.ClientProfiles.FindAsync(clientId);
            if (cliente == null)
                return (false, "El cliente no existe.");

            // Comprueba si ya está inscripto
            var esta = clase.clients?.Any(cp => cp.idClient == clientId) ?? false;
            if (esta)
                return (false, "El cliente ya está inscripto en la clase.");

            // Comprueba capacidad
            var alumnosActuales = clase.clients?.Count ?? 0;
            if (alumnosActuales >= clase.capacityMax)
                return (false, "La clase alcanzó su capacidad máxima.");

            // Añade el cliente a la colección
            clase.clients.Add(cliente);
            await _context.SaveChangesAsync();

            return (true, "Alumno agregado correctamente a la clase.");
        }

        public async Task<(bool success, string message)> RegistrarAsistencia(int idClass, List<AssistanceDTO> dtos)
        {
            var clase = await _context.Classes.FindAsync(idClass);
            if (clase == null)
                return (false, "La clase especificada no existe.");

            if (!clase.isActive)
                return (false, "No se puede tomar asistencia: La clase se encuentra cancelada.");

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
                    date = DateTimeOffset.Now.DateTime,
                    isAssisted = dto.isAssisted,
                    observations = dto.observations
                };
                _context.Set<Assistance>().Add(nuevaAsistencia);
            }


            await _context.SaveChangesAsync();
            return (true, $"Se registraron {dtos.Count} asistencias correctamente.");
        }
        public async Task<(bool success, string message)> VerificarCapacidad(int courtId, int capacityMax)
        {
            var cancha = await _context.Courts
                .Include(c => c.courtType)
                .FirstOrDefaultAsync(c => c.idCourt == courtId);

            if (cancha == null)
                return (false, "La cancha especificada no existe.");

            if (capacityMax <= 0)
                return (false, "La capacidad máxima debe ser mayor que cero.");

            if (cancha.courtType != null && capacityMax > cancha.courtType.capacity)
                return (false, $"La capacidad máxima ({capacityMax}) excede la capacidad de la cancha ({cancha.courtType.capacity}).");

            return (true, string.Empty);
        }

        // Extra: Listar clases
        public async Task<List<ClassResponseDTO>> ListarClases()
        {
            return await _context.Classes
                .Select(c => new ClassResponseDTO
                {
                    idClass = c.idClass,
                    name = c.name,
                    description = c.description,
                    classType = c.classType,
                    profesorId = c.profesorId,

                    professorFullName = (c.profesor.personalClubProfile.user.name + " " + c.profesor.personalClubProfile.user.lastName).Trim(),

                    courtId = c.courtId,
                    courtName = c.court.name ?? "",
                    date = c.date,
                    capacityMax = c.capacityMax,
                    currentAlumnosCount = c.clients.Count,
                    duration = c.duration,
                    price = c.price,
                    isActive = c.isActive
                })
                .ToListAsync();
        }
    }
}