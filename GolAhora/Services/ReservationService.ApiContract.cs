using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class ReservationService
    {

        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Reservations
                .Include(r => r.client).ThenInclude(c => c.user)
                .Include(r => r.court)
                .Include(r => r.payment)
                .ToListAsync();
            return Ok(reservas.Select(ProjectReserva));
        }

        public async Task<IActionResult> CreateReserva([FromBody] JsonElement body)
        {
            var idCourt = ReadInt(body, "idCourt") ?? ReadNestedInt(body, "cancha", "idCancha") ?? ReadNestedInt(body, "cancha", "id") ?? 0;
            var idClient = ReadInt(body, "idClient") ?? ReadNestedInt(body, "cliente", "idClient") ?? ReadNestedInt(body, "cliente", "idCliente") ?? 0;
            idClient = idClient == 0 ? await ClientProfileIdFromUser(ReadNestedInt(body, "cliente", "idUsuario")) : idClient;
            var date = ReadDate(body, "reservationDate") ?? ReadDate(body, "fechaUso") ?? DateTime.Today;
            var start = ReadTime(body, "startTime") ?? ReadTime(body, "horaInicio") ?? TimeSpan.Zero;
            var end = ReadTime(body, "endTime") ?? ReadTime(body, "horaFin") ?? start.Add(TimeSpan.FromHours(1));
            var validation = await ValidateReservation(idClient, idCourt, date, start, end);
            if (validation is not null) return ValidationError(validation);

            var amount = ReadDouble(body, "montoTotal") ?? await CalcularMontoApiContract(idCourt, start, end);

            var payment = new Payments
            {
                idClient = idClient,
                amount = amount,
                paymentDate = DateTime.UtcNow,
                paymentMethod = ReadNestedString(body, "cobro", "metodo") ?? "Pendiente",
                isSuccessful = ReadNestedString(body, "cobro", "estado") == "pagado"
            };
            _context.Payments.Add(payment);
            await _unitOfWork.SaveChangesAsync();

            var reserva = new Reservation
            {
                idClient = idClient,
                idCourt = idCourt,
                reservationDate = date,
                startTime = start,
                endTime = end,
                status = payment.isSuccessful ? "confirmada" : "pendiente",
                createdAt = DateTime.UtcNow,
                totalPrice = amount,
                idPayment = payment.idPayment,
                isPaid = payment.isSuccessful
            };
            _context.Reservations.Add(reserva);
            await _unitOfWork.SaveChangesAsync();
            var full = await _context.Reservations.Include(r => r.client).ThenInclude(c => c.user).Include(r => r.court).Include(r => r.payment).FirstAsync(r => r.idReservation == reserva.idReservation);
            return Ok(ProjectReserva(full));
        }

        public async Task<IActionResult> UpdateReserva(int id, [FromBody] JsonElement body)
        {
            var reserva = await _context.Reservations.Include(r => r.payment).FirstOrDefaultAsync(r => r.idReservation == id);
            if (reserva is null) return NotFound();
            var nextDate = ReadDate(body, "reservationDate") ?? ReadDate(body, "fechaUso") ?? reserva.reservationDate;
            var nextStart = ReadTime(body, "startTime") ?? ReadTime(body, "horaInicio") ?? reserva.startTime;
            var nextEnd = ReadTime(body, "endTime") ?? ReadTime(body, "horaFin") ?? reserva.endTime;
            var validation = await ValidateReservation(reserva.idClient, reserva.idCourt, nextDate, nextStart, nextEnd, id);
            if (validation is not null) return ValidationError(validation);

            reserva.reservationDate = nextDate;
            reserva.startTime = nextStart;
            reserva.endTime = nextEnd;
            reserva.totalPrice = ReadDouble(body, "montoTotal") ?? reserva.totalPrice;
            reserva.status = ReadString(body, "estado") ?? reserva.status;
            reserva.isPaid = ReadNestedString(body, "cobro", "estado") == "pagado" || reserva.status == "confirmada" || reserva.isPaid;
            if (reserva.payment != null)
            {
                reserva.payment.amount = reserva.totalPrice;
                reserva.payment.isSuccessful = reserva.isPaid;
                reserva.payment.paymentMethod = ReadNestedString(body, "cobro", "metodo") ?? reserva.payment.paymentMethod;
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idReserva = reserva.idReservation });
        }

        public async Task<IActionResult> ConfirmarReserva(int id, [FromBody] JsonElement body)
        {
            var reserva = await _context.Reservations.Include(r => r.payment).FirstOrDefaultAsync(r => r.idReservation == id);
            if (reserva is null) return NotFound();
            reserva.isPaid = true;
            reserva.status = "confirmada";
            if (reserva.payment != null)
            {
                reserva.payment.isSuccessful = true;
                reserva.payment.paymentMethod = ReadString(body, "metodo") ?? reserva.payment.paymentMethod;
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idReserva = id, estado = "confirmada" });
        }

        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservations.Include(r => r.payment).FirstOrDefaultAsync(r => r.idReservation == id);
            if (reserva is null) return NotFound();
            reserva.status = "cancelada";
            reserva.isPaid = false;
            if (reserva.payment != null) reserva.payment.isSuccessful = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idReserva = id, estado = "cancelada" });
        }

        public async Task<IActionResult> GetClases()
        {
            var clases = await _context.Classes
                .Include(c => c.profesor).ThenInclude(p => p.personalClubProfile).ThenInclude(p => p.user)
                .Include(c => c.court)
                .Include(c => c.clients).ThenInclude(c => c.user)
                .ToListAsync();
            return Ok(clases.Select(c => new
            {
                idClase = c.idClass,
                nombre = c.name,
                descripcion = c.description,
                tipoClase = c.classType,
                profesor = c.profesor == null ? null : new { id = c.profesorId, nombre = c.profesor.personalClubProfile.user.name, apellido = c.profesor.personalClubProfile.user.lastName },
                cancha = c.court.name,
                canchaId = c.courtId,
                fecha = c.date.ToString("yyyy-MM-dd"),
                horario = c.date.ToString("HH:mm"),
                duracionMin = c.duration,
                maxAlumnos = c.capacityMax,
                precio = c.price,
                estado = c.isActive ? "programada" : "cancelada",
                alumnos = c.clients.Select(a => new { id = a.idClient, nombre = $"{a.user.name} {a.user.lastName}", email = a.user.Email, presente = false })
            }));
        }

        public async Task<IActionResult> CreateClase([FromBody] JsonElement body)
        {
            var date = ReadDate(body, "fecha") ?? ReadDate(body, "date") ?? DateTime.Today;
            var time = ReadTime(body, "horario") ?? TimeSpan.Zero;
            var professorId = await ResolveProfessorProfileId(body);
            var courtId = await ResolveCourtId(body);
            var duration = ReadInt(body, "duracionMin") ?? ReadInt(body, "duration") ?? 60;
            var capacity = ReadInt(body, "maxAlumnos") ?? ReadInt(body, "capacityMax") ?? 1;
            var validation = await ValidateClass(body, professorId, courtId, date.Date + time, duration, capacity);
            if (validation is not null) return ValidationError(validation);

            var clase = new Class
            {
                name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "",
                description = ReadString(body, "descripcion") ?? ReadString(body, "description") ?? "",
                classType = ReadString(body, "tipoClase") ?? ReadString(body, "classType") ?? "",
                profesorId = professorId,
                courtId = courtId,
                date = date.Date + time,
                duration = duration,
                capacityMax = capacity,
                price = ReadDouble(body, "precio") ?? ReadDouble(body, "price") ?? 0,
                isActive = true
            };
            _context.Classes.Add(clase);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = clase.idClass });
        }

        public async Task<IActionResult> UpdateClase(int id, [FromBody] JsonElement body)
        {
            var clase = await _context.Classes.FindAsync(id);
            if (clase is null) return NotFound();
            var nextDate = ReadDate(body, "fecha") ?? clase.date.Date;
            var nextTime = ReadTime(body, "horario") ?? clase.date.TimeOfDay;
            var nextDuration = ReadInt(body, "duracionMin") ?? clase.duration;
            var nextCapacity = ReadInt(body, "maxAlumnos") ?? clase.capacityMax;
            var nextProfessorId = await ResolveProfessorProfileId(body, clase.profesorId);
            var nextCourtId = await ResolveCourtId(body, clase.courtId);
            var validation = await ValidateClass(body, nextProfessorId, nextCourtId, nextDate.Date + nextTime, nextDuration, nextCapacity, id);
            if (validation is not null) return ValidationError(validation);

            clase.name = ReadString(body, "nombre") ?? clase.name;
            clase.description = ReadString(body, "descripcion") ?? clase.description;
            clase.classType = ReadString(body, "tipoClase") ?? clase.classType;
            clase.profesorId = nextProfessorId;
            clase.courtId = nextCourtId;
            clase.date = nextDate.Date + nextTime;
            clase.capacityMax = nextCapacity;
            clase.duration = nextDuration;
            clase.price = ReadDouble(body, "precio") ?? clase.price;
            clase.isActive = ReadString(body, "estado") == "cancelada" ? false : clase.isActive;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = id });
        }

        public async Task<IActionResult> CancelarClase(int id)
        {
            var clase = await _context.Classes.FindAsync(id);
            if (clase is null) return NotFound();
            clase.isActive = !clase.isActive;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = id, estado = clase.isActive ? "programada" : "cancelada" });
        }

        public async Task<IActionResult> AddAlumno(int id, int clientId)
        {
            var clase = await _context.Classes.Include(c => c.clients).FirstOrDefaultAsync(c => c.idClass == id);
            var client = await _context.ClientProfiles.FindAsync(clientId);
            if (clase is null || client is null) return NotFound();
            if (!clase.isActive) return ValidationError("No se puede inscribir a una clase cancelada.");
            if (clase.clients.Count >= clase.capacityMax) return ValidationError("La clase no tiene cupos disponibles.");
            if (!clase.clients.Any(c => c.idClient == clientId)) clase.clients.Add(client);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = id });
        }

        public async Task<IActionResult> SaveAsistencia(int id, [FromBody] JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Array) return BadRequest();
            foreach (var item in body.EnumerateArray())
            {
                _context.Assistances.Add(new Assistance
                {
                    classId = id,
                    clientId = ReadInt(item, "idClient") ?? ReadInt(item, "id") ?? 0,
                    date = DateTime.UtcNow,
                    isAssisted = ReadBool(item, "presente") ?? ReadBool(item, "isAssisted") ?? false,
                    observations = ReadString(item, "observations") ?? ""
                });
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = id });
        }

        public async Task<IActionResult> GetAsistencias()
        {
            var asistencias = await _context.Assistances
                .Include(a => a.client).ThenInclude(c => c.user)
                .Include(a => a.clas)
                .ToListAsync();

            return Ok(asistencias.Select(a => new
            {
                idAsistencia = a.idAssistance,
                idClase = a.classId,
                clase = a.clas.name,
                idClient = a.clientId,
                alumno = $"{a.client.user.name} {a.client.user.lastName}",
                fecha = a.date,
                presente = a.isAssisted,
                observaciones = a.observations
            }));
        }
    }
}





