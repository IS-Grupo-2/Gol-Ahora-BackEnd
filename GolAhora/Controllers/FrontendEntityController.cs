using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api")]
    public class FrontendEntityController : ControllerBase
    {
        private readonly AppContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public FrontendEntityController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
            _userManager = userManager;
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var users = await _context.Users
                .Include(u => u.clientProfile)
                .Where(u => u.clientProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idCliente = u.clientProfile!.idClient,
                idClient = u.clientProfile.idClient,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "cliente",
                nroSocio = $"C-{u.clientProfile.numberPartner:000}",
                numberPartner = u.clientProfile.numberPartner,
                idTeam = u.clientProfile.idTeam
            }));
        }

        [HttpPost("clientes")]
        public async Task<IActionResult> CreateCliente([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "Client");
            var partnerNumber = await _context.ClientProfiles.CountAsync() + 1;
            _context.ClientProfiles.Add(new ClientProfile { idUser = user.Id, numberPartner = partnerNumber });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        [HttpPut("clientes/{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        [HttpPatch("clientes/{id}/estado")]
        [HttpPut("clientes/{id}/estado")]
        public async Task<IActionResult> ToggleCliente(int id) => await ToggleUser(id);

        [HttpGet("empleados")]
        public async Task<IActionResult> GetEmpleados()
        {
            var users = await _context.Users
                .Include(u => u.personalClubProfile)!.ThenInclude(p => p!.employeeProfile)
                .Where(u => u.personalClubProfile != null && u.personalClubProfile.employeeProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idEmployee = u.personalClubProfile!.employeeProfile!.idEmployee,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "empleado",
                legajo = u.personalClubProfile.legajo,
                turno = u.personalClubProfile.turno,
                sector = u.personalClubProfile.employeeProfile.sector
            }));
        }

        [HttpPost("empleados")]
        public async Task<IActionResult> CreateEmpleado([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);
            validation = ValidateStaffPayload(body, requireSector: true);
            if (validation is not null) return ValidationError(validation);

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "PersonalClubProfile");
            await EnsureRole(user, "Employee");
            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = ReadString(body, "legajo") ?? $"E-{user.Id:000}",
                startDate = ReadDate(body, "startDate") ?? DateTime.UtcNow,
                turno = ReadString(body, "turno") ?? ""
            };
            _context.PersonalClubProfiles.Add(profile);
            await _unitOfWork.SaveChangesAsync();
            _context.EmployeeProfiles.Add(new EmployeeProfile { idPersonalClub = profile.idPersonalClub, sector = ReadString(body, "sector") ?? "" });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        [HttpPut("empleados/{id}")]
        public async Task<IActionResult> UpdateEmpleado(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        [HttpPatch("empleados/{id}/estado")]
        [HttpPut("empleados/{id}/estado")]
        public async Task<IActionResult> ToggleEmpleado(int id) => await ToggleUser(id);

        [HttpGet("profesores")]
        public async Task<IActionResult> GetProfesores()
        {
            var users = await _context.Users
                .Include(u => u.personalClubProfile)!.ThenInclude(p => p!.professorProfile)!.ThenInclude(p => p!.certifications)
                .Where(u => u.personalClubProfile != null && u.personalClubProfile.professorProfile != null)
                .ToListAsync();

            return Ok(users.Select(u => new
            {
                idUsuario = u.Id,
                id = u.Id,
                idProfessor = u.personalClubProfile!.professorProfile!.idProfessor,
                nombre = u.name,
                apellido = u.lastName,
                dni = u.DNI,
                email = u.Email,
                telefono = u.PhoneNumber,
                username = u.UserName,
                activo = u.isActive,
                estado = u.isActive ? "activo" : "inactivo",
                fechaRegistro = u.registerDate,
                rol = "profesor",
                legajo = u.personalClubProfile.legajo,
                turno = u.personalClubProfile.turno,
                especialidad = u.personalClubProfile.professorProfile.speciality,
                speciality = u.personalClubProfile.professorProfile.speciality,
                certification = u.personalClubProfile.professorProfile.certification,
                certificaciones = u.personalClubProfile.professorProfile.certifications.Select(c => new
                {
                    id = c.idCertification,
                    nombre = c.name,
                    institucion = c.institution,
                    numero = c.numberCertificate,
                    fecha = c.dateObtained,
                    verificada = c.verified
                })
            }));
        }

        [HttpPost("profesores")]
        public async Task<IActionResult> CreateProfesor([FromBody] JsonElement body)
        {
            var validation = await ValidateUserPayload(body, requirePassword: true);
            if (validation is not null) return ValidationError(validation);
            validation = ValidateStaffPayload(body, requireSector: false);
            if (validation is not null) return ValidationError(validation);
            if ((ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? "").Trim().Length < 2) return ValidationError("Especialidad invalida.");

            var user = BuildUser(body);
            var password = ReadString(body, "password") ?? "123456";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await EnsureRole(user, "PersonalClubProfile");
            await EnsureRole(user, "Professor");
            var profile = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = ReadString(body, "legajo") ?? $"P-{user.Id:000}",
                startDate = ReadDate(body, "startDate") ?? DateTime.UtcNow,
                turno = ReadString(body, "turno") ?? ""
            };
            _context.PersonalClubProfiles.Add(profile);
            await _unitOfWork.SaveChangesAsync();
            _context.ProfessorProfiles.Add(new ProfessorProfile
            {
                idPersonalClub = profile.idPersonalClub,
                speciality = ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? "",
                certification = ReadString(body, "certification") ?? ""
            });
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(user.Id))!);
        }

        [HttpPut("profesores/{id}")]
        public async Task<IActionResult> UpdateProfesor(int id, [FromBody] JsonElement body) => await UpdateUser(id, body);

        [HttpPatch("profesores/{id}/estado")]
        [HttpPut("profesores/{id}/estado")]
        public async Task<IActionResult> ToggleProfesor(int id) => await ToggleUser(id);

        [HttpGet("tipos-canchas")]
        public async Task<IActionResult> GetTiposCanchas()
        {
            var items = await _context.CourtTypes.ToListAsync();
            return Ok(items.Select(t => new
            {
                id = t.idTypeCourt,
                idTipoCancha = t.idTypeCourt,
                nombre = t.name,
                superficie = t.superficie.ToString(),
                capacidadJugadores = t.capacity,
                duracionMaxReservaMin = DurationByCourtType(t.name),
                precioHora = t.pricePerHour,
                descripcion = t.description
            }));
        }

        [HttpPost("tipos-canchas")]
        public async Task<IActionResult> CreateTipoCancha([FromBody] JsonElement body)
        {
            var validation = ValidateCourtTypePayload(body);
            if (validation is not null) return ValidationError(validation);

            var type = new CourtType
            {
                name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "",
                superficie = ReadDouble(body, "superficie") ?? 1,
                capacity = ReadInt(body, "capacidadJugadores") ?? ReadInt(body, "capacity") ?? 1,
                pricePerHour = ReadDouble(body, "precioHora") ?? ReadDouble(body, "pricePerHour") ?? 0,
                description = ReadString(body, "descripcion") ?? ReadString(body, "description") ?? ""
            };
            _context.CourtTypes.Add(type);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = type.idTypeCourt, nombre = type.name });
        }

        [HttpPut("tipos-canchas/{id}")]
        public async Task<IActionResult> UpdateTipoCancha(int id, [FromBody] JsonElement body)
        {
            var type = await _context.CourtTypes.FindAsync(id);
            if (type is null) return NotFound();
            var validation = ValidateCourtTypePayload(body);
            if (validation is not null) return ValidationError(validation);

            type.name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? type.name;
            type.superficie = ReadDouble(body, "superficie") ?? type.superficie;
            type.capacity = ReadInt(body, "capacidadJugadores") ?? type.capacity;
            type.pricePerHour = ReadDouble(body, "precioHora") ?? type.pricePerHour;
            type.description = ReadString(body, "descripcion") ?? type.description;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = type.idTypeCourt, nombre = type.name });
        }

        [HttpDelete("tipos-canchas/{id}")]
        public async Task<IActionResult> DeleteTipoCancha(int id)
        {
            var type = await _context.CourtTypes.Include(t => t.courts).FirstOrDefaultAsync(t => t.idTypeCourt == id);
            if (type is null) return NotFound();
            foreach (var court in type.courts) court.isAvailable = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpGet("canchas")]
        public async Task<IActionResult> GetCanchas()
        {
            var courts = await _context.Courts.Include(c => c.courtType).Include(c => c.disponibilities).ToListAsync();
            return Ok(courts.Select(c => new
            {
                id = c.idCourt,
                idCancha = c.idCourt,
                numero = c.idCourt,
                nombre = c.name,
                idTipo = c.courtTypeId,
                tipoCanchaId = c.courtTypeId,
                tipoCancha = c.courtType.name,
                estado = c.isAvailable ? (c.disponibilities.Any(d => !d.isAvailable) ? "mantenimiento" : "activa") : "inactiva",
                activa = c.isAvailable,
                descripcion = c.description
            }));
        }

        [HttpPost("canchas")]
        public async Task<IActionResult> CreateCancha([FromBody] JsonElement body)
        {
            var validation = await ValidateCourtPayload(body);
            if (validation is not null) return ValidationError(validation);

            var court = new Court
            {
                name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "",
                description = ReadString(body, "descripcion") ?? ReadString(body, "description") ?? "",
                imageUrl = ReadString(body, "imageUrl") ?? "",
                isAvailable = true,
                courtTypeId = ReadInt(body, "idTipo") ?? ReadInt(body, "tipoCanchaId") ?? ReadInt(body, "courtTypeId") ?? 0
            };
            _context.Courts.Add(court);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = court.idCourt, idCancha = court.idCourt, nombre = court.name });
        }

        [HttpPut("canchas/{id}")]
        public async Task<IActionResult> UpdateCancha(int id, [FromBody] JsonElement body)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court is null) return NotFound();
            var validation = await ValidateCourtPayload(body);
            if (validation is not null) return ValidationError(validation);

            court.name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? court.name;
            court.description = ReadString(body, "descripcion") ?? court.description;
            court.courtTypeId = ReadInt(body, "idTipo") ?? ReadInt(body, "tipoCanchaId") ?? court.courtTypeId;
            court.isAvailable = ReadBool(body, "activa") ?? (ReadString(body, "estado") is "inactiva" ? false : court.isAvailable);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = court.idCourt, idCancha = court.idCourt, nombre = court.name });
        }

        [HttpDelete("canchas/{id}")]
        public async Task<IActionResult> ToggleCancha(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court is null) return NotFound();
            court.isAvailable = !court.isAvailable;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = court.idCourt, activa = court.isAvailable, estado = court.isAvailable ? "activa" : "inactiva" });
        }

        [HttpGet("disponibilidades")]
        public async Task<IActionResult> GetDisponibilidades()
        {
            var items = await _context.Disponibilities.ToListAsync();
            return Ok(items.Select(d => new
            {
                id = d.idDisponibility,
                idCancha = d.courtId,
                canchaId = d.courtId,
                diaSemana = DayName(d.day),
                horaInicio = d.startTime.Hours + d.startTime.Minutes / 60.0,
                horaFin = d.endTime.Hours + d.endTime.Minutes / 60.0,
                disponible = d.isAvailable
            }));
        }

        [HttpPost("disponibilidades")]
        public async Task<IActionResult> CreateDisponibilidad([FromBody] JsonElement body)
        {
            var disp = BuildDisponibility(body);
            var validation = await ValidateDisponibility(disp);
            if (validation is not null) return ValidationError(validation);

            _context.Disponibilities.Add(disp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = disp.idDisponibility });
        }

        [HttpPut("disponibilidades/{id}")]
        public async Task<IActionResult> UpdateDisponibilidad(int id, [FromBody] JsonElement body)
        {
            var disp = await _context.Disponibilities.FindAsync(id);
            if (disp is null) return NotFound();
            var next = BuildDisponibility(body);
            var validation = await ValidateDisponibility(next, id);
            if (validation is not null) return ValidationError(validation);

            disp.courtId = next.courtId;
            disp.day = next.day;
            disp.startTime = next.startTime;
            disp.endTime = next.endTime;
            disp.isAvailable = next.isAvailable;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = disp.idDisponibility });
        }

        [HttpDelete("disponibilidades/{id}")]
        public async Task<IActionResult> DeleteDisponibilidad(int id)
        {
            var disp = await _context.Disponibilities.FindAsync(id);
            if (disp is null) return NotFound();
            _context.Disponibilities.Remove(disp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpGet("reservas")]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Reservations
                .Include(r => r.client).ThenInclude(c => c.user)
                .Include(r => r.court)
                .Include(r => r.payment)
                .ToListAsync();
            return Ok(reservas.Select(ProjectReserva));
        }

        [HttpPost("reservas")]
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

            var amount = ReadDouble(body, "montoTotal") ?? await CalcularMonto(idCourt, start, end);

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

        [HttpPut("reservas/{id}")]
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

        [HttpPut("reservas/{id}/confirmar")]
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

        [HttpPut("reservas/{id}/cancelar")]
        [HttpDelete("reservas/{id}")]
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

        [HttpGet("clases")]
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

        [HttpPost("clases")]
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

        [HttpPut("clases/{id}")]
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

        [HttpPut("clases/{id}/cancelar")]
        public async Task<IActionResult> CancelarClase(int id)
        {
            var clase = await _context.Classes.FindAsync(id);
            if (clase is null) return NotFound();
            clase.isActive = !clase.isActive;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idClase = id, estado = clase.isActive ? "programada" : "cancelada" });
        }

        [HttpPost("clases/{id}/agregar-alumno/{clientId}")]
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

        [HttpPost("clases/{id}/asistencia")]
        [HttpPut("asistencias/{id}")]
        [HttpPost("asistencias/{id}")]
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

        [HttpGet("asistencias")]
        public async Task<IActionResult> GetAsistencias()
        {
            var asistencias = await _context.Assistances.ToListAsync();
            return Ok(asistencias.GroupBy(a => a.classId).ToDictionary(g => g.Key.ToString(), g => g.Select(a => new { id = a.clientId, presente = a.isAssisted, observations = a.observations })));
        }

        [HttpGet("cobros")]
        public async Task<IActionResult> GetCobros()
        {
            var pagos = await _context.Payments
                .Include(p => p.client).ThenInclude(c => c.user)
                .Include(p => p.discount)
                .Include(p => p.reservation)
                .ToListAsync();
            return Ok(pagos.Select(p => new
            {
                idCobro = p.idPayment,
                idReserva = p.reservation?.idReservation,
                cliente = new { idUsuario = p.client.idUser, idClient = p.idClient, nombre = p.client.user.name, apellido = p.client.user.lastName, dni = p.client.user.DNI },
                concepto = p.reservation == null ? "Cobro" : $"Reserva #{p.reservation.idReservation}",
                tipoCobro = p.reservation == null ? "Servicio" : "Reserva Cancha",
                monto = p.amount + (p.discount?.discountValue ?? 0),
                descuento = p.discount?.nombre,
                montoFinal = p.amount,
                fecha = p.paymentDate.ToString("yyyy-MM-dd"),
                estado = p.isSuccessful ? "pagado" : "pendiente",
                metodo = p.paymentMethod
            }));
        }

        [HttpPost("cobros")]
        public async Task<IActionResult> CreateCobro([FromBody] JsonElement body)
        {
            var validation = await ValidatePaymentPayload(body);
            if (validation is not null) return ValidationError(validation);

            var pago = new Payments
            {
                idClient = ReadNestedInt(body, "cliente", "idClient") ?? ReadNestedInt(body, "cliente", "idCliente") ?? ReadInt(body, "idClient") ?? 0,
                amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? 0,
                paymentDate = ReadDate(body, "fecha") ?? DateTime.UtcNow,
                paymentMethod = ReadString(body, "metodo") ?? "No informado",
                isSuccessful = ReadString(body, "estado") == "pagado",
                idDiscount = ReadInt(body, "idDiscount")
            };
            _context.Payments.Add(pago);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = pago.idPayment });
        }

        [HttpPut("cobros/{id}")]
        public async Task<IActionResult> UpdateCobro(int id, [FromBody] JsonElement body)
        {
            var pago = await _context.Payments.FindAsync(id);
            if (pago is null) return NotFound();
            var validation = await ValidatePaymentPayload(body, existingClientId: pago.idClient);
            if (validation is not null) return ValidationError(validation);

            pago.amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? pago.amount;
            pago.paymentMethod = ReadString(body, "metodo") ?? pago.paymentMethod;
            pago.isSuccessful = ReadString(body, "estado") == "pagado";
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = id });
        }

        [HttpDelete("cobros/{id}")]
        public async Task<IActionResult> DeleteCobro(int id)
        {
            var pago = await _context.Payments.FindAsync(id);
            if (pago is null) return NotFound();
            pago.isSuccessful = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = id });
        }

        [HttpGet("descuentos")]
        public async Task<IActionResult> GetDescuentos()
        {
            var descuentos = await _context.Discounts.ToListAsync();
            return Ok(descuentos.Select(d => new
            {
                id = d.idDiscount,
                codigo = d.discountType,
                nombre = d.nombre,
                porcentaje = d.discountValue,
                descripcion = d.conditions,
                activo = d.startDate <= DateTime.UtcNow && d.endDate >= DateTime.UtcNow
            }));
        }

        [HttpPost("descuentos")]
        public async Task<IActionResult> CreateDescuento([FromBody] JsonElement body)
        {
            var validation = ValidateDiscountPayload(body);
            if (validation is not null) return ValidationError(validation);

            var descuento = new Discounts
            {
                nombre = ReadString(body, "nombre") ?? "",
                discountType = ReadString(body, "codigo") ?? ReadString(body, "discountType") ?? "",
                discountValue = ReadDouble(body, "porcentaje") ?? ReadDouble(body, "discountValue") ?? 0,
                conditions = ReadString(body, "descripcion") ?? ReadString(body, "conditions") ?? "",
                startDate = DateTime.UtcNow,
                endDate = ReadBool(body, "activo") == false ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddYears(1)
            };
            _context.Discounts.Add(descuento);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = descuento.idDiscount });
        }

        [HttpPut("descuentos/{id}")]
        public async Task<IActionResult> UpdateDescuento(int id, [FromBody] JsonElement body)
        {
            var descuento = await _context.Discounts.FindAsync(id);
            if (descuento is null) return NotFound();
            var validation = ValidateDiscountPayload(body);
            if (validation is not null) return ValidationError(validation);

            descuento.nombre = ReadString(body, "nombre") ?? descuento.nombre;
            descuento.discountType = ReadString(body, "codigo") ?? descuento.discountType;
            descuento.discountValue = ReadDouble(body, "porcentaje") ?? descuento.discountValue;
            descuento.conditions = ReadString(body, "descripcion") ?? descuento.conditions;
            descuento.endDate = ReadBool(body, "activo") == false ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddYears(1);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpDelete("descuentos/{id}")]
        public async Task<IActionResult> DeleteDescuento(int id)
        {
            var descuento = await _context.Discounts.FindAsync(id);
            if (descuento is null) return NotFound();
            descuento.endDate = DateTime.UtcNow.AddDays(-1);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpGet("recibos")]
        public async Task<IActionResult> GetRecibos()
        {
            var recibos = await _context.Receipts.Include(r => r.payment).ThenInclude(p => p.client).ThenInclude(c => c.user).ToListAsync();
            return Ok(recibos.Select(r => new
            {
                idRecibo = r.idReceipt,
                nroRecibo = r.receiptNumber,
                cobro = new { idCobro = r.idPayment, montoFinal = r.payment.amount },
                cliente = new { idUsuario = r.payment.client.idUser, nombre = r.payment.client.user.name, apellido = r.payment.client.user.lastName, dni = r.payment.client.user.DNI },
                pago = new { metodoPago = r.payment.paymentMethod, fechaPago = r.payment.paymentDate, estado = r.payment.isSuccessful ? "Completado" : "Pendiente" },
                fecha = r.date.ToString("yyyy-MM-dd"),
                total = r.totalAmount,
                detalles = r.details,
                estado = "emitido"
            }));
        }

        [HttpPost("recibos")]
        public async Task<IActionResult> CreateRecibo([FromBody] JsonElement body)
        {
            var paymentId = ReadNestedInt(body, "cobro", "idCobro") ?? ReadInt(body, "idPayment") ?? 0;
            if (!await _context.Payments.AnyAsync(p => p.idPayment == paymentId)) return ValidationError("Cobro inexistente.");
            var total = ReadDouble(body, "total") ?? ReadNestedDouble(body, "cobro", "montoFinal") ?? 0;
            if (total < 0) return ValidationError("El total del recibo no puede ser negativo.");

            var recibo = new Receipts
            {
                idPayment = paymentId,
                receiptNumber = ReadString(body, "nroRecibo") ?? $"0001-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100000000:00000000}",
                totalAmount = ReadDouble(body, "total") ?? ReadNestedDouble(body, "cobro", "montoFinal") ?? 0,
                details = ReadString(body, "detalles") ?? "",
                date = ReadDate(body, "fecha") ?? DateTime.UtcNow
            };
            _context.Receipts.Add(recibo);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = recibo.idReceipt });
        }

        [HttpPut("recibos/{id}")]
        public async Task<IActionResult> UpdateRecibo(int id, [FromBody] JsonElement body)
        {
            var recibo = await _context.Receipts.FindAsync(id);
            if (recibo is null) return NotFound();
            recibo.details = ReadString(body, "detalles") ?? recibo.details;
            recibo.totalAmount = ReadDouble(body, "total") ?? recibo.totalAmount;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = id });
        }

        [HttpDelete("recibos/{id}")]
        public async Task<IActionResult> DeleteRecibo(int id)
        {
            var recibo = await _context.Receipts.FindAsync(id);
            if (recibo is null) return NotFound();
            _context.Receipts.Remove(recibo);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = id });
        }

        [HttpGet("competencias")]
        public async Task<IActionResult> GetCompetencias()
        {
            var comps = await _context.Competences.Include(c => c.teams).ToListAsync();
            return Ok(comps.Select(c => new
            {
                id = c.idCompetence,
                nombre = c.name,
                descripcion = c.description,
                tipo = c is Tournament ? "torneo" : "liga",
                estado = c.isActive ? "inscripcion" : "finalizado",
                maxEquipos = c.capacityTeams,
                equipos = c.teams.Select(t => t.idTeam),
                fechaInicio = c.startDate.ToString("yyyy-MM-dd"),
                fechaFin = c.endDate.ToString("yyyy-MM-dd"),
                precioInscripcion = 0
            }));
        }

        [HttpPost("competencias")]
        public async Task<IActionResult> CreateCompetencia([FromBody] JsonElement body)
        {
            var validation = ValidateCompetencePayload(body, requireFutureStart: true);
            if (validation is not null) return ValidationError(validation);

            Competence comp = ReadString(body, "tipo") == "torneo" ? new Tournament() : new League();
            comp.name = ReadString(body, "nombre") ?? "";
            comp.description = ReadString(body, "descripcion") ?? "";
            comp.startDate = ReadDate(body, "fechaInicio") ?? DateTime.UtcNow;
            comp.endDate = ReadDate(body, "fechaFin") ?? DateTime.UtcNow.AddDays(1);
            comp.isActive = ReadString(body, "estado") != "finalizado";
            comp.regulations = ReadString(body, "regulations") ?? "";
            comp.capacityTeams = ReadInt(body, "maxEquipos") ?? 2;
            _context.Competences.Add(comp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = comp.idCompetence });
        }

        [HttpPut("competencias/{id}")]
        public async Task<IActionResult> UpdateCompetencia(int id, [FromBody] JsonElement body)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return NotFound();
            var validation = ValidateCompetencePayload(body);
            if (validation is not null) return ValidationError(validation);

            comp.name = ReadString(body, "nombre") ?? comp.name;
            comp.description = ReadString(body, "descripcion") ?? comp.description;
            comp.startDate = ReadDate(body, "fechaInicio") ?? comp.startDate;
            comp.endDate = ReadDate(body, "fechaFin") ?? comp.endDate;
            comp.capacityTeams = ReadInt(body, "maxEquipos") ?? comp.capacityTeams;
            comp.isActive = ReadString(body, "estado") != "finalizado";
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpDelete("competencias/{id}")]
        public async Task<IActionResult> DeleteCompetencia(int id)
        {
            var comp = await _context.Competences.FindAsync(id);
            if (comp is null) return NotFound();
            comp.isActive = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        [HttpPost("competencias/{competenciaId}/inscribir")]
        public async Task<IActionResult> InscribirEquipo(int competenciaId, [FromBody] JsonElement body)
        {
            var equipoId = ReadInt(body, "equipoId") ?? ReadInt(body, "idEquipo") ?? 0;
            var comp = await _context.Competences.Include(c => c.teams).FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null || !await _context.Teams.AnyAsync(t => t.idTeam == equipoId)) return NotFound();
            if (!comp.isActive) return ValidationError("La competencia no esta abierta a inscripciones.");
            if (comp.teams.Count >= comp.capacityTeams) return ValidationError("La competencia no tiene cupos disponibles.");
            if (!await _context.CompetenceTeams.AnyAsync(ct => ct.idCompetence == competenciaId && ct.idTeam == equipoId))
            {
                var payment = new Payments { idClient = await FirstClientId(), amount = 0, paymentDate = DateTime.UtcNow, paymentMethod = "Inscripcion", isSuccessful = true };
                _context.Payments.Add(payment);
                await _unitOfWork.SaveChangesAsync();
                _context.CompetenceTeams.Add(new CompetenceTeam { idCompetence = competenciaId, idTeam = equipoId, inscription = DateTime.UtcNow, status = true, idPayment = payment.idPayment });
                await _unitOfWork.SaveChangesAsync();
            }
            return Ok(new { id = competenciaId });
        }

        [HttpGet("equipos")]
        public async Task<IActionResult> GetEquipos()
        {
            var teams = await _context.Teams.Include(t => t.captain).ThenInclude(c => c!.user).Include(t => t.players).ThenInclude(p => p.user).ToListAsync();
            return Ok(teams.Select(t => new
            {
                idEquipo = t.idTeam,
                nombre = t.name,
                capitan = t.captain == null ? "" : $"{t.captain.user.name} {t.captain.user.lastName}",
                integrantes = t.players.Select(p => $"{p.user.name} {p.user.lastName}"),
                creadoPor = t.captain == null ? null : new { idUsuario = t.captain.idUser, nombre = t.captain.user.name, apellido = t.captain.user.lastName, email = t.captain.user.Email },
                fechaCreacion = ""
            }));
        }

        [HttpPost("equipos")]
        public async Task<IActionResult> CreateEquipo([FromBody] JsonElement body)
        {
            var validation = await ValidateTeamPayload(body);
            if (validation is not null) return ValidationError(validation);

            var team = new Team { name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "", clientId = ReadNestedInt(body, "creadoPor", "idClient") ?? ReadInt(body, "clientId") };
            _context.Teams.Add(team);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = team.idTeam });
        }

        [HttpPut("equipos/{id}")]
        public async Task<IActionResult> UpdateEquipo(int id, [FromBody] JsonElement body)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return NotFound();
            var validation = await ValidateTeamPayload(body, id);
            if (validation is not null) return ValidationError(validation);

            team.name = ReadString(body, "nombre") ?? team.name;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = id });
        }

        [HttpDelete("equipos/{id}")]
        public async Task<IActionResult> DeleteEquipo(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return NotFound();
            _context.Teams.Remove(team);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idEquipo = id });
        }

        [HttpGet("fixtures")]
        public async Task<IActionResult> GetFixtures()
        {
            var matches = await _context.Matches.Include(m => m.result).ToListAsync();
            return Ok(matches.GroupBy(m => m.idCompetence).Select(g => new
            {
                competenciaID = g.Key,
                rondas = g.GroupBy(m => m.round).Select(r => new
                {
                    numero = r.Key,
                    partidos = r.Select(m => new
                    {
                        idPartido = m.idMatch,
                        equipoLocalId = m.idTeamA,
                        equipoVisitanteId = m.idTeamB,
                        fecha = m.date.ToString("yyyy-MM-dd"),
                        estado = m.isPlayed ? "finalizado" : "programado",
                        definitivo = m.isPlayed,
                        resultado = m.result == null ? null : new { local = m.result.scoreTeamLocal, visitante = m.result.scoreTeamVisitor }
                    })
                })
            }));
        }

        [HttpPost("competencias/{competenciaId}/fixture")]
        public async Task<IActionResult> GenerarFixture(int competenciaId)
        {
            var comp = await _context.Competences
                .Include(c => c.teams)
                .FirstOrDefaultAsync(c => c.idCompetence == competenciaId);
            if (comp is null) return NotFound();

            var courtId = await _context.Courts.Select(c => c.idCourt).FirstOrDefaultAsync();
            if (courtId == 0) return BadRequest(new { message = "Debe existir al menos una cancha para generar fixture" });

            var teamIds = comp.teams.Select(t => t.idTeam).ToList();
            if (teamIds.Count < 2) return BadRequest(new { message = "La competencia necesita al menos dos equipos" });

            var previous = await _context.Matches.Include(m => m.result).Where(m => m.idCompetence == competenciaId).ToListAsync();
            _context.Results.RemoveRange(previous.Where(m => m.result != null).Select(m => m.result!));
            _context.Matches.RemoveRange(previous);

            var date = comp.startDate == default ? DateTime.UtcNow.Date : comp.startDate.Date;
            var round = 1;
            var nuevosPartidos = new List<Match>();
            for (var i = 0; i < teamIds.Count; i += 2)
            {
                if (i + 1 >= teamIds.Count) break;
                var match = new Match
                {
                    idCompetence = competenciaId,
                    round = round,
                    idTeamA = teamIds[i],
                    idTeamB = teamIds[i + 1],
                    idCourt = courtId,
                    date = date.AddDays(i / 2),
                    isPlayed = false
                };
                nuevosPartidos.Add(match);
                _context.Matches.Add(match);
            }

            comp.isActive = true;
            await _unitOfWork.SaveChangesAsync();
            foreach (var match in nuevosPartidos)
            {
                match.idResults = match.idMatch;
            }
            await _unitOfWork.SaveChangesAsync();
            return await GetFixtures();
        }

        [HttpPatch("fixtures/{competenciaId}/partido/{partidoId}/resultado")]
        public async Task<IActionResult> RegistrarResultado(int competenciaId, int partidoId, [FromBody] JsonElement body)
        {
            var match = await _context.Matches.Include(m => m.result).FirstOrDefaultAsync(m => m.idMatch == partidoId && m.idCompetence == competenciaId);
            if (match is null) return NotFound();

            var resultadoNode = body.TryGetProperty("resultado", out var nested) ? nested : body;
            var local = ReadInt(resultadoNode, "local") ?? ReadInt(resultadoNode, "golesLocal") ?? ReadInt(resultadoNode, "scoreTeamLocal") ?? 0;
            var visitante = ReadInt(resultadoNode, "visitante") ?? ReadInt(resultadoNode, "golesVisitante") ?? ReadInt(resultadoNode, "scoreTeamVisitor") ?? 0;
            if (local < 0 || visitante < 0) return ValidationError("Los goles no pueden ser negativos.");

            if (match.result is null)
            {
                _context.Results.Add(new Result
                {
                    idResults = match.idMatch,
                    idMatch = match.idMatch,
                    scoreTeamLocal = local,
                    scoreTeamVisitor = visitante
                });
            }
            else
            {
                match.result.scoreTeamLocal = local;
                match.result.scoreTeamVisitor = visitante;
            }

            match.isPlayed = true;
            await _unitOfWork.SaveChangesAsync();
            return await GetFixtures();
        }

        [HttpGet("reportes/ingresos")]
        public async Task<IActionResult> ReporteIngresos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var from = desde ?? DateTime.MinValue;
            var to = hasta ?? DateTime.MaxValue;
            var pagos = await _context.Payments.Where(p => p.paymentDate >= from && p.paymentDate <= to && p.isSuccessful).ToListAsync();
            var total = pagos.Sum(p => p.amount);
            return Ok(new
            {
                totalIngresos = total,
                ingresosPorConcepto = pagos.GroupBy(p => p.paymentMethod).Select(g => new { concepto = g.Key, monto = g.Sum(p => p.amount), porcentaje = total == 0 ? 0 : Math.Round(g.Sum(p => p.amount) * 100 / total, 2), color = "fill-green" })
            });
        }

        [HttpGet("reportes/asistencias")]
        public async Task<IActionResult> ReporteAsistencias([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var asistencias = await _context.Assistances.Include(a => a.clas).ToListAsync();
            return Ok(new
            {
                totalAsistencias = asistencias.Count,
                asistenciasPorClase = asistencias.GroupBy(a => a.clas.name).Select(g => new { clase = g.Key, asistentes = g.Count(a => a.isAssisted), capacidad = g.Count(), porcentaje = g.Count() == 0 ? 0 : Math.Round(g.Count(a => a.isAssisted) * 100.0 / g.Count(), 2), color = "fill-blue" })
            });
        }

        [HttpGet("reportes/reservas")]
        public async Task<IActionResult> ReporteReservas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var reservas = await _context.Reservations.Include(r => r.court).ToListAsync();
            var total = reservas.Count;
            return Ok(new
            {
                totalReservas = total,
                reservasPorEstado = reservas.GroupBy(r => r.isPaid ? "Completadas" : "Pendientes").Select(g => new { estado = g.Key, cantidad = g.Count(), porcentaje = total == 0 ? 0 : Math.Round(g.Count() * 100.0 / total, 2), color = "fill-green" }),
                reservasPorCancha = reservas.GroupBy(r => r.court.name).Select(g => new { cancha = g.Key, cantidad = g.Count(), porcentaje = total == 0 ? 0 : Math.Round(g.Count() * 100.0 / total, 2), color = "fill-purple" })
            });
        }

        [HttpPut("usuarios/{id}/password")]
        [HttpPut("User/{id}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] JsonElement body)
        {
            var currentPassword = ReadString(body, "currentPassword") ?? ReadString(body, "actual") ?? "";
            var newPassword = ReadString(body, "newPassword") ?? ReadString(body, "nueva") ?? "";
            var confirmPassword = ReadString(body, "confirmPassword") ?? ReadString(body, "confirmacion") ?? newPassword;

            if (string.IsNullOrWhiteSpace(currentPassword)) return ValidationError("Ingrese la contrasena actual.");
            if (newPassword.Length < 6) return ValidationError("La nueva contrasena debe tener al menos 6 caracteres.");
            if (newPassword != confirmPassword) return ValidationError("La confirmacion no coincide con la nueva contrasena.");

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded) return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new { message = "Contrasena actualizada correctamente." });
        }

        [HttpPost("soporte")]
        public IActionResult SendSupportMessage([FromBody] JsonElement body)
        {
            var message = ReadString(body, "mensaje") ?? ReadString(body, "message") ?? "";
            if (message.Trim().Length < 10) return ValidationError("El mensaje debe tener al menos 10 caracteres.");
            return Ok(new { message = "Mensaje recibido." });
        }

        private IActionResult ValidationError(string message) => BadRequest(new { message });

        private async Task<string?> ValidateUserPayload(JsonElement body, int? existingUserId = null, bool requirePassword = false)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var lastName = (ReadString(body, "apellido") ?? ReadString(body, "lastName") ?? "").Trim();
            var dni = (ReadString(body, "dni") ?? ReadString(body, "DNI") ?? "").Trim();
            var phone = (ReadString(body, "telefono") ?? ReadString(body, "phoneNumber") ?? "").Trim();
            var email = (ReadString(body, "email") ?? "").Trim();
            var username = (ReadString(body, "username") ?? ReadString(body, "userName") ?? email).Trim();
            var password = ReadString(body, "password") ?? "";

            if (name.Length < 2) return "Nombre muy corto.";
            if (lastName.Length < 2) return "Apellido muy corto.";
            if (body.TryGetProperty("fechaNacimiento", out _) && ReadDate(body, "fechaNacimiento") is null) return "Ingrese fecha de nacimiento valida.";
            if (body.TryGetProperty("nroSocio", out _) && (ReadString(body, "nroSocio") ?? "").Trim().Length < 4) return "Numero de socio invalido.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dni, @"^\d{7,8}$")) return "DNI invalido. Debe tener 7 u 8 digitos.";
            if (phone.Length < 8 || phone.Length > 20) return "Telefono invalido.";
            if (!email.Contains('@') || !email.Contains('.')) return "Email invalido.";
            if (username.Length < 4 || username.Length > 20) return "El usuario debe tener entre 4 y 20 caracteres.";
            if (requirePassword && password.Length < 6) return "La contrasena debe tener al menos 6 caracteres.";

            var normalizedEmail = email.ToUpperInvariant();
            var normalizedUserName = username.ToUpperInvariant();
            var duplicate = await _context.Users.AnyAsync(u =>
                (!existingUserId.HasValue || u.Id != existingUserId.Value) &&
                (u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedUserName || u.DNI == dni));
            if (duplicate) return "Ya existe un usuario con ese email, nombre de usuario o DNI.";

            return null;
        }

        private static string? ValidateStaffPayload(JsonElement body, bool requireSector)
        {
            var legajo = (ReadString(body, "legajo") ?? "").Trim();
            var turno = (ReadString(body, "turno") ?? "").Trim();
            var sector = (ReadString(body, "sector") ?? "").Trim();

            if (legajo.Length < 4) return "Legajo invalido.";
            if (turno.Length == 0) return "Seleccione un turno.";
            if (requireSector && sector.Length < 2) return "Sector invalido.";
            return null;
        }

        private static string? ValidateCourtTypePayload(JsonElement body)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var surface = (ReadString(body, "superficie") ?? "").Trim();
            var capacity = ReadInt(body, "capacidadJugadores") ?? ReadInt(body, "capacity") ?? 0;
            var duration = ReadInt(body, "duracionMaxReservaMin") ?? 60;
            var price = ReadDouble(body, "precioHora") ?? ReadDouble(body, "pricePerHour") ?? -1;

            if (name.Length < 2) return "Nombre de tipo de cancha muy corto.";
            if (surface.Length < 1) return "Superficie requerida.";
            if (capacity < 2) return "La capacidad minima es de 2 jugadores.";
            if (duration < 30) return "La duracion maxima debe ser de al menos 30 minutos.";
            if (price < 0) return "Precio por hora invalido.";
            return null;
        }

        private async Task<string?> ValidateCourtPayload(JsonElement body)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var courtTypeId = ReadInt(body, "idTipo") ?? ReadInt(body, "tipoCanchaId") ?? ReadInt(body, "courtTypeId") ?? 0;

            if (name.Length < 2) return "Nombre de cancha muy corto.";
            if (!await _context.CourtTypes.AnyAsync(t => t.idTypeCourt == courtTypeId)) return "Tipo de cancha inexistente.";
            return null;
        }

        private async Task<string?> ValidateDisponibility(Disponibility disp, int? existingId = null)
        {
            if (!await _context.Courts.AnyAsync(c => c.idCourt == disp.courtId)) return "Cancha inexistente.";
            if (disp.startTime < TimeSpan.FromHours(9) || disp.startTime > TimeSpan.FromHours(23)) return "La hora de inicio debe estar entre 09:00 y 23:00.";
            if (disp.endTime <= disp.startTime) return "La hora de fin debe ser posterior al inicio.";
            if (disp.endTime > TimeSpan.FromHours(24)) return "La hora de fin no puede superar las 24:00.";

            var overlap = await _context.Disponibilities.AnyAsync(d =>
                d.courtId == disp.courtId &&
                d.day == disp.day &&
                (!existingId.HasValue || d.idDisponibility != existingId.Value) &&
                disp.startTime < d.endTime &&
                disp.endTime > d.startTime);
            return overlap ? "Solapamiento con otra franja horaria." : null;
        }

        private async Task<string?> ValidateReservation(int clientId, int courtId, DateTime date, TimeSpan start, TimeSpan end, int? existingReservationId = null)
        {
            if (!await _context.ClientProfiles.AnyAsync(c => c.idClient == clientId)) return "Cliente inexistente.";
            var court = await _context.Courts.Include(c => c.courtType).FirstOrDefaultAsync(c => c.idCourt == courtId);
            if (court is null) return "Cancha inexistente.";
            if (!court.isAvailable) return "La cancha seleccionada no esta activa.";
            if (end <= start) return "La hora de fin debe ser posterior a la hora de inicio.";
            if ((end - start).TotalMinutes < 30) return "La reserva debe tener una duracion minima de 30 minutos.";

            var today = DateTime.Today;
            if (date.Date < today) return "La fecha de la reserva no puede ser anterior a hoy.";
            if (date.Date > today.AddDays(30)) return "Solo se pueden crear reservas con hasta 30 dias de anticipacion.";
            if ((end - start).TotalMinutes > DurationByCourtType(court.courtType.name)) return $"El tipo de cancha {court.courtType.name} no permite reservas tan largas.";

            var hasSlot = await _context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == date.DayOfWeek &&
                d.isAvailable &&
                start >= d.startTime &&
                end <= d.endTime);
            if (!hasSlot) return "La cancha no tiene disponibilidad habilitada para ese horario.";

            var blockedSlot = await _context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == date.DayOfWeek &&
                !d.isAvailable &&
                start < d.endTime &&
                end > d.startTime);
            if (blockedSlot) return "La cancha esta bloqueada o en mantenimiento en ese horario.";

            var reservationOverlap = await _context.Reservations.AnyAsync(r =>
                r.idCourt == courtId &&
                r.reservationDate.Date == date.Date &&
                r.status != "cancelada" &&
                (!existingReservationId.HasValue || r.idReservation != existingReservationId.Value) &&
                start < r.endTime &&
                end > r.startTime);
            if (reservationOverlap) return "Ya existe una reserva para esa cancha en la franja horaria elegida.";

            var classOverlap = await _context.Classes.AnyAsync(c =>
                c.courtId == courtId &&
                c.date.Date == date.Date &&
                c.isActive &&
                start < c.date.TimeOfDay.Add(TimeSpan.FromMinutes(c.duration)) &&
                end > c.date.TimeOfDay);
            return classOverlap ? "Ya existe una clase para esa cancha en la franja horaria elegida." : null;
        }

        private async Task<string?> ValidateClass(JsonElement body, int professorId, int courtId, DateTime date, int duration, int capacity, int? existingClassId = null)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var type = (ReadString(body, "tipoClase") ?? ReadString(body, "classType") ?? "").Trim();
            var price = ReadDouble(body, "precio") ?? ReadDouble(body, "price") ?? 0;

            if (name.Length < 3) return "Nombre de clase muy corto.";
            if (type.Length == 0) return "Seleccione un tipo de clase.";
            if (professorId <= 0 || !await _context.ProfessorProfiles.AnyAsync(p => p.idProfessor == professorId)) return "Profesor inexistente.";
            if (!await _context.Courts.AnyAsync(c => c.idCourt == courtId && c.isAvailable)) return "Cancha inexistente o inactiva.";
            if (date.Date < DateTime.Today) return "La fecha de la clase no puede ser anterior a hoy.";
            if (duration <= 0) return "La duracion de la clase debe ser mayor a cero.";
            if (capacity < 1) return "La clase debe admitir al menos un alumno.";
            if (price < 0) return "El precio no puede ser negativo.";

            var start = date.TimeOfDay;
            var end = start.Add(TimeSpan.FromMinutes(duration));
            var hasSlot = await _context.Disponibilities.AnyAsync(d => d.courtId == courtId && d.day == date.DayOfWeek && d.isAvailable && start >= d.startTime && end <= d.endTime);
            if (!hasSlot) return "La cancha no tiene disponibilidad habilitada para ese horario.";

            var reservationOverlap = await _context.Reservations.AnyAsync(r => r.idCourt == courtId && r.reservationDate.Date == date.Date && r.status != "cancelada" && start < r.endTime && end > r.startTime);
            if (reservationOverlap) return "Ya existe una reserva para esa cancha en la franja horaria elegida.";

            var classOverlap = await _context.Classes.AnyAsync(c =>
                c.courtId == courtId &&
                c.date.Date == date.Date &&
                c.isActive &&
                (!existingClassId.HasValue || c.idClass != existingClassId.Value) &&
                start < c.date.TimeOfDay.Add(TimeSpan.FromMinutes(c.duration)) &&
                end > c.date.TimeOfDay);
            return classOverlap ? "Ya existe una clase para esa cancha en la franja horaria elegida." : null;
        }

        private async Task<string?> ValidatePaymentPayload(JsonElement body, int? existingClientId = null)
        {
            var clientId = ReadNestedInt(body, "cliente", "idClient") ?? ReadNestedInt(body, "cliente", "idCliente") ?? ReadInt(body, "idClient") ?? existingClientId ?? 0;
            clientId = clientId == 0 ? await ClientProfileIdFromUser(ReadNestedInt(body, "cliente", "idUsuario")) : clientId;
            var amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? 0;
            var concept = (ReadString(body, "concepto") ?? "").Trim();
            var type = (ReadString(body, "tipoCobro") ?? "").Trim();
            var date = ReadDate(body, "fecha");

            if (!await _context.ClientProfiles.AnyAsync(c => c.idClient == clientId)) return "Cliente inexistente.";
            if (concept.Length > 0 && concept.Length < 3) return "Concepto muy corto.";
            if (body.TryGetProperty("tipoCobro", out _) && type.Length == 0) return "Seleccione el tipo de cobro.";
            if (amount <= 0) return "Monto invalido.";
            if (body.TryGetProperty("fecha", out _) && date is null) return "Ingrese una fecha valida.";
            return null;
        }

        private static string? ValidateDiscountPayload(JsonElement body)
        {
            var code = (ReadString(body, "codigo") ?? ReadString(body, "discountType") ?? "").Trim();
            var name = (ReadString(body, "nombre") ?? "").Trim();
            var percentage = ReadDouble(body, "porcentaje") ?? ReadDouble(body, "discountValue") ?? 0;

            if (code.Length < 2) return "Codigo de descuento invalido.";
            if (name.Length < 3) return "Ingrese un nombre de descuento valido.";
            if (percentage <= 0 || percentage > 100) return "El porcentaje debe estar entre 1 y 100.";
            return null;
        }

        private static string? ValidateCompetencePayload(JsonElement body, bool requireFutureStart = false)
        {
            var name = (ReadString(body, "nombre") ?? "").Trim();
            var type = ReadString(body, "tipo") ?? "liga";
            var start = ReadDate(body, "fechaInicio");
            var maxTeams = ReadInt(body, "maxEquipos") ?? 0;

            if (name.Length < 3) return "Ingrese un nombre de competencia valido.";
            if (start is null) return "Ingrese fecha de inicio.";
            if (requireFutureStart && start.Value.Date <= DateTime.Today) return "La fecha de inicio debe ser posterior al dia de hoy.";
            if (maxTeams < 2) return "La competencia necesita al menos 2 equipos.";
            if (type == "torneo" && !(maxTeams == 2 || maxTeams == 4 || maxTeams == 8 || maxTeams == 16)) return "Los torneos admiten 2, 4, 8 o 16 equipos.";
            if (type != "torneo" && maxTeams > 20) return "Las ligas admiten entre 2 y 20 equipos.";
            return null;
        }

        private async Task<string?> ValidateTeamPayload(JsonElement body, int? existingTeamId = null)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            if (name.Length < 2) return "Nombre de equipo invalido.";

            var duplicate = await _context.Teams.AnyAsync(t => t.name == name && (!existingTeamId.HasValue || t.idTeam != existingTeamId.Value));
            return duplicate ? "Ya existe un equipo con ese nombre." : null;
        }

        private async Task<int> ResolveProfessorProfileId(JsonElement body, int fallback = 0)
        {
            var id = ReadNestedInt(body, "profesor", "idProfessor") ?? ReadInt(body, "profesorId");
            if (id.HasValue && await _context.ProfessorProfiles.AnyAsync(p => p.idProfessor == id.Value)) return id.Value;

            var userId = ReadNestedInt(body, "profesor", "idUsuario") ?? ReadNestedInt(body, "profesor", "id") ?? ReadInt(body, "idProfesor");
            if (userId.HasValue)
            {
                var profileId = await _context.ProfessorProfiles
                    .Where(p => p.personalClubProfile.idUser == userId.Value)
                    .Select(p => p.idProfessor)
                    .FirstOrDefaultAsync();
                if (profileId != 0) return profileId;
            }

            return fallback != 0 ? fallback : await _context.ProfessorProfiles.Select(p => p.idProfessor).FirstOrDefaultAsync();
        }

        private async Task<int> ResolveCourtId(JsonElement body, int fallback = 0)
        {
            var id = ReadInt(body, "canchaId") ?? ReadInt(body, "courtId") ?? ReadNestedInt(body, "cancha", "idCancha") ?? ReadNestedInt(body, "cancha", "id");
            if (id.HasValue) return id.Value;

            var name = ReadString(body, "cancha");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var courtId = await _context.Courts.Where(c => c.name == name).Select(c => c.idCourt).FirstOrDefaultAsync();
                if (courtId != 0) return courtId;
            }

            return fallback;
        }

        private async Task<int> ClientProfileIdFromUser(int? userId)
        {
            if (!userId.HasValue) return 0;
            return await _context.ClientProfiles.Where(c => c.idUser == userId.Value).Select(c => c.idClient).FirstOrDefaultAsync();
        }

        private async Task<object?> ProjectUser(int id)
        {
            var user = await _context.Users.Include(u => u.clientProfile).Include(u => u.personalClubProfile)!.ThenInclude(p => p!.employeeProfile).Include(u => u.personalClubProfile)!.ThenInclude(p => p!.professorProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return null;
            return new
            {
                idUsuario = user.Id,
                id = user.Id,
                nombre = user.name,
                apellido = user.lastName,
                dni = user.DNI,
                email = user.Email,
                telefono = user.PhoneNumber,
                username = user.UserName,
                activo = user.isActive,
                estado = user.isActive ? "activo" : "inactivo"
            };
        }

        private async Task<IActionResult> UpdateUser(int id, JsonElement body)
        {
            var user = await _context.Users.Include(u => u.personalClubProfile).ThenInclude(p => p!.employeeProfile).Include(u => u.personalClubProfile).ThenInclude(p => p!.professorProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return NotFound();
            var validation = await ValidateUserPayload(body, existingUserId: id);
            if (validation is not null) return ValidationError(validation);
            if (user.personalClubProfile?.employeeProfile != null)
            {
                validation = ValidateStaffPayload(body, requireSector: true);
                if (validation is not null) return ValidationError(validation);
            }
            if (user.personalClubProfile?.professorProfile != null)
            {
                validation = ValidateStaffPayload(body, requireSector: false);
                if (validation is not null) return ValidationError(validation);
                if ((ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? user.personalClubProfile.professorProfile.speciality).Trim().Length < 2) return ValidationError("Especialidad invalida.");
            }

            user.name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? user.name;
            user.lastName = ReadString(body, "apellido") ?? ReadString(body, "lastName") ?? user.lastName;
            user.DNI = ReadString(body, "dni") ?? ReadString(body, "DNI") ?? user.DNI;
            user.Email = ReadString(body, "email") ?? user.Email;
            user.UserName = ReadString(body, "username") ?? ReadString(body, "userName") ?? user.UserName;
            user.PhoneNumber = ReadString(body, "telefono") ?? ReadString(body, "phoneNumber") ?? user.PhoneNumber;
            if (user.personalClubProfile != null)
            {
                user.personalClubProfile.turno = ReadString(body, "turno") ?? user.personalClubProfile.turno;
                if (user.personalClubProfile.employeeProfile != null) user.personalClubProfile.employeeProfile.sector = ReadString(body, "sector") ?? user.personalClubProfile.employeeProfile.sector;
                if (user.personalClubProfile.professorProfile != null) user.personalClubProfile.professorProfile.speciality = ReadString(body, "especialidad") ?? ReadString(body, "speciality") ?? user.personalClubProfile.professorProfile.speciality;
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(id))!);
        }

        private async Task<IActionResult> ToggleUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null) return NotFound();
            user.isActive = !user.isActive;
            await _unitOfWork.SaveChangesAsync();
            return Ok((await ProjectUser(id))!);
        }

        private static User BuildUser(JsonElement body)
        {
            var email = ReadString(body, "email") ?? "";
            return new User
            {
                name = ReadString(body, "nombre") ?? ReadString(body, "name") ?? "",
                lastName = ReadString(body, "apellido") ?? ReadString(body, "lastName") ?? "",
                DNI = ReadString(body, "dni") ?? ReadString(body, "DNI") ?? "",
                Email = email,
                UserName = ReadString(body, "username") ?? ReadString(body, "userName") ?? email,
                PhoneNumber = ReadString(body, "telefono") ?? ReadString(body, "phoneNumber") ?? "",
                isActive = true,
                registerDate = DateTime.UtcNow
            };
        }

        private async Task EnsureRole(User user, string role)
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        private Disponibility BuildDisponibility(JsonElement body)
        {
            return new Disponibility
            {
                courtId = ReadInt(body, "idCancha") ?? ReadInt(body, "canchaId") ?? ReadInt(body, "courtId") ?? 0,
                day = ParseDay(ReadString(body, "diaSemana")) ?? DayOfWeek.Monday,
                startTime = HourToTime(ReadDouble(body, "horaInicio") ?? 0),
                endTime = HourToTime(ReadDouble(body, "horaFin") ?? 0),
                isAvailable = ReadBool(body, "disponible") ?? true
            };
        }

        private object ProjectReserva(Reservation r)
        {
            return new
            {
                idReserva = r.idReservation,
                cliente = new { idUsuario = r.client.idUser, idClient = r.idClient, nombre = r.client.user.name, apellido = r.client.user.lastName },
                reservador = new { id = r.client.idUser, nombre = $"{r.client.user.name} {r.client.user.lastName}", email = r.client.user.Email, rol = "cliente" },
                cancha = new { id = r.idCourt, idCancha = r.idCourt, nombre = r.court.name, numero = r.idCourt },
                fechaCreacion = r.createdAt,
                fechaUso = r.reservationDate.ToString("yyyy-MM-dd"),
                horaInicio = r.startTime.ToString(@"hh\:mm"),
                horaFin = r.endTime.ToString(@"hh\:mm"),
                duracionMin = (int)(r.endTime - r.startTime).TotalMinutes,
                estado = string.IsNullOrWhiteSpace(r.status) ? (r.isPaid ? "confirmada" : "pendiente") : r.status,
                montoTotal = r.totalPrice,
                cobro = new { estado = r.status == "cancelada" ? "cancelado" : (r.isPaid ? "pagado" : "pendiente"), metodo = r.payment.paymentMethod }
            };
        }

        private async Task<double> CalcularMonto(int courtId, TimeSpan start, TimeSpan end)
        {
            var court = await _context.Courts.Include(c => c.courtType).FirstOrDefaultAsync(c => c.idCourt == courtId);
            return court == null ? 0 : Math.Max(0, (end - start).TotalHours) * court.courtType.pricePerHour;
        }

        private async Task<int> FirstClientId()
        {
            return await _context.ClientProfiles.Select(c => c.idClient).FirstOrDefaultAsync();
        }

        private static int DurationByCourtType(string name)
        {
            var lower = name.ToLowerInvariant();
            if (lower.Contains("5")) return 60;
            if (lower.Contains("7")) return 90;
            return 120;
        }

        private static string DayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miercoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sabado",
            _ => "Domingo"
        };

        private static DayOfWeek? ParseDay(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "lunes" => DayOfWeek.Monday,
                "martes" => DayOfWeek.Tuesday,
                "miercoles" or "miércoles" => DayOfWeek.Wednesday,
                "jueves" => DayOfWeek.Thursday,
                "viernes" => DayOfWeek.Friday,
                "sabado" or "sábado" => DayOfWeek.Saturday,
                "domingo" => DayOfWeek.Sunday,
                _ => null
            };
        }

        private static TimeSpan HourToTime(double value)
        {
            var hours = (int)Math.Floor(value);
            var minutes = (int)Math.Round((value - hours) * 60);
            return new TimeSpan(hours, minutes, 0);
        }

        private static string? ReadString(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
        }

        private static int? ReadInt(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)) return value;
            return int.TryParse(property.ToString(), out value) ? value : null;
        }

        private static double? ReadDouble(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)) return value;
            return double.TryParse(property.ToString(), out value) ? value : null;
        }

        private static bool? ReadBool(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.True) return true;
            if (property.ValueKind == JsonValueKind.False) return false;
            return bool.TryParse(property.ToString(), out var value) ? value : null;
        }

        private static DateTime? ReadDate(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return DateTime.TryParse(property.ToString(), out var value) ? value : null;
        }

        private static TimeSpan? ReadTime(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return TimeSpan.TryParse(property.ToString(), out var value) ? value : null;
        }

        private static int? ReadNestedInt(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadInt(node, name) : null;
        }

        private static string? ReadNestedString(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadString(node, name) : null;
        }

        private static double? ReadNestedDouble(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadDouble(node, name) : null;
        }
    }
}

