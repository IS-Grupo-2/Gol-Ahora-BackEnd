using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    [NonController]
    public abstract class ServicePayloadBase : ControllerBase
    {
        protected abstract AppContext Context { get; }
        protected virtual IUnitOfWork? UnitOfWork => null;
        protected virtual UserManager<User>? IdentityUserManager => null;
        protected IActionResult ValidationError(string message) => BadRequest(new { message });

        protected async Task<string?> ValidateUserPayload(JsonElement body, int? existingUserId = null, bool requirePassword = false)
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
            var duplicate = await Context.Users.AnyAsync(u =>
                (!existingUserId.HasValue || u.Id != existingUserId.Value) &&
                (u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedUserName || u.DNI == dni));
            if (duplicate) return "Ya existe un usuario con ese email, nombre de usuario o DNI.";

            return null;
        }

        protected static string? ValidateStaffPayload(JsonElement body, bool requireSector)
        {
            var legajo = (ReadString(body, "legajo") ?? "").Trim();
            var turno = (ReadString(body, "turno") ?? "").Trim();
            var sector = (ReadString(body, "sector") ?? "").Trim();

            if (legajo.Length < 4) return "Legajo invalido.";
            if (turno.Length == 0) return "Seleccione un turno.";
            if (requireSector && sector.Length < 2) return "Sector invalido.";
            return null;
        }

        protected static string? ValidateCourtTypePayload(JsonElement body)
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

        protected async Task<string?> ValidateCourtPayload(JsonElement body)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var courtTypeId = ReadInt(body, "idTipo") ?? ReadInt(body, "tipoCanchaId") ?? ReadInt(body, "courtTypeId") ?? 0;

            if (name.Length < 2) return "Nombre de cancha muy corto.";
            if (!await Context.CourtTypes.AnyAsync(t => t.idTypeCourt == courtTypeId)) return "Tipo de cancha inexistente.";
            return null;
        }

        protected async Task<string?> ValidateDisponibility(Disponibility disp, int? existingId = null)
        {
            if (!await Context.Courts.AnyAsync(c => c.idCourt == disp.courtId)) return "Cancha inexistente.";
            if (disp.startTime < TimeSpan.FromHours(9) || disp.startTime > TimeSpan.FromHours(23)) return "La hora de inicio debe estar entre 09:00 y 23:00.";
            if (disp.endTime <= disp.startTime) return "La hora de fin debe ser posterior al inicio.";
            if (disp.endTime > TimeSpan.FromHours(24)) return "La hora de fin no puede superar las 24:00.";

            var overlap = await Context.Disponibilities.AnyAsync(d =>
                d.courtId == disp.courtId &&
                d.day == disp.day &&
                (!existingId.HasValue || d.idDisponibility != existingId.Value) &&
                disp.startTime < d.endTime &&
                disp.endTime > d.startTime);
            return overlap ? "Solapamiento con otra franja horaria." : null;
        }

        protected async Task<string?> ValidateReservation(int clientId, int courtId, DateTime date, TimeSpan start, TimeSpan end, int? existingReservationId = null)
        {
            if (!await Context.ClientProfiles.AnyAsync(c => c.idClient == clientId)) return "Cliente inexistente.";
            var court = await Context.Courts.Include(c => c.courtType).FirstOrDefaultAsync(c => c.idCourt == courtId);
            if (court is null) return "Cancha inexistente.";
            if (!court.isAvailable) return "La cancha seleccionada no esta activa.";
            if (end <= start) return "La hora de fin debe ser posterior a la hora de inicio.";
            if ((end - start).TotalMinutes < 30) return "La reserva debe tener una duracion minima de 30 minutos.";

            var today = DateTime.Today;
            if (date.Date < today) return "La fecha de la reserva no puede ser anterior a hoy.";
            if (date.Date > today.AddDays(30)) return "Solo se pueden crear reservas con hasta 30 dias de anticipacion.";
            if ((end - start).TotalMinutes > DurationByCourtType(court.courtType.name)) return $"El tipo de cancha {court.courtType.name} no permite reservas tan largas.";

            var hasSlot = await Context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == date.DayOfWeek &&
                d.isAvailable &&
                start >= d.startTime &&
                end <= d.endTime);
            if (!hasSlot) return "La cancha no tiene disponibilidad habilitada para ese horario.";

            var blockedSlot = await Context.Disponibilities.AnyAsync(d =>
                d.courtId == courtId &&
                d.day == date.DayOfWeek &&
                !d.isAvailable &&
                start < d.endTime &&
                end > d.startTime);
            if (blockedSlot) return "La cancha esta bloqueada o en mantenimiento en ese horario.";

            var reservationOverlap = await Context.Reservations.AnyAsync(r =>
                r.idCourt == courtId &&
                r.reservationDate.Date == date.Date &&
                r.status != "cancelada" &&
                (!existingReservationId.HasValue || r.idReservation != existingReservationId.Value) &&
                start < r.endTime &&
                end > r.startTime);
            if (reservationOverlap) return "Ya existe una reserva para esa cancha en la franja horaria elegida.";

            var classOverlap = await Context.Classes.AnyAsync(c =>
                c.courtId == courtId &&
                c.date.Date == date.Date &&
                c.isActive &&
                start < c.date.TimeOfDay.Add(TimeSpan.FromMinutes(c.duration)) &&
                end > c.date.TimeOfDay);
            return classOverlap ? "Ya existe una clase para esa cancha en la franja horaria elegida." : null;
        }

        protected async Task<string?> ValidateClass(JsonElement body, int professorId, int courtId, DateTime date, int duration, int capacity, int? existingClassId = null)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            var type = (ReadString(body, "tipoClase") ?? ReadString(body, "classType") ?? "").Trim();
            var price = ReadDouble(body, "precio") ?? ReadDouble(body, "price") ?? 0;

            if (name.Length < 3) return "Nombre de clase muy corto.";
            if (type.Length == 0) return "Seleccione un tipo de clase.";
            if (professorId <= 0 || !await Context.ProfessorProfiles.AnyAsync(p => p.idProfessor == professorId)) return "Profesor inexistente.";
            if (!await Context.Courts.AnyAsync(c => c.idCourt == courtId && c.isAvailable)) return "Cancha inexistente o inactiva.";
            if (date.Date < DateTime.Today) return "La fecha de la clase no puede ser anterior a hoy.";
            if (duration <= 0) return "La duracion de la clase debe ser mayor a cero.";
            if (capacity < 1) return "La clase debe admitir al menos un alumno.";
            if (price < 0) return "El precio no puede ser negativo.";

            var start = date.TimeOfDay;
            var end = start.Add(TimeSpan.FromMinutes(duration));
            var hasSlot = await Context.Disponibilities.AnyAsync(d => d.courtId == courtId && d.day == date.DayOfWeek && d.isAvailable && start >= d.startTime && end <= d.endTime);
            if (!hasSlot) return "La cancha no tiene disponibilidad habilitada para ese horario.";

            var reservationOverlap = await Context.Reservations.AnyAsync(r => r.idCourt == courtId && r.reservationDate.Date == date.Date && r.status != "cancelada" && start < r.endTime && end > r.startTime);
            if (reservationOverlap) return "Ya existe una reserva para esa cancha en la franja horaria elegida.";

            var classOverlap = await Context.Classes.AnyAsync(c =>
                c.courtId == courtId &&
                c.date.Date == date.Date &&
                c.isActive &&
                (!existingClassId.HasValue || c.idClass != existingClassId.Value) &&
                start < c.date.TimeOfDay.Add(TimeSpan.FromMinutes(c.duration)) &&
                end > c.date.TimeOfDay);
            return classOverlap ? "Ya existe una clase para esa cancha en la franja horaria elegida." : null;
        }

        protected async Task<string?> ValidatePaymentPayload(JsonElement body, int? existingClientId = null)
        {
            var clientId = ReadNestedInt(body, "cliente", "idClient") ?? ReadNestedInt(body, "cliente", "idCliente") ?? ReadInt(body, "idClient") ?? existingClientId ?? 0;
            clientId = clientId == 0 ? await ClientProfileIdFromUser(ReadNestedInt(body, "cliente", "idUsuario")) : clientId;
            var amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? 0;
            var concept = (ReadString(body, "concepto") ?? "").Trim();
            var type = (ReadString(body, "tipoCobro") ?? "").Trim();
            var date = ReadDate(body, "fecha");

            if (!await Context.ClientProfiles.AnyAsync(c => c.idClient == clientId)) return "Cliente inexistente.";
            if (concept.Length > 0 && concept.Length < 3) return "Concepto muy corto.";
            if (body.TryGetProperty("tipoCobro", out _) && type.Length == 0) return "Seleccione el tipo de cobro.";
            if (amount <= 0) return "Monto invalido.";
            if (body.TryGetProperty("fecha", out _) && date is null) return "Ingrese una fecha valida.";
            return null;
        }

        protected static string? ValidateDiscountPayload(JsonElement body)
        {
            var code = (ReadString(body, "codigo") ?? ReadString(body, "discountType") ?? "").Trim();
            var name = (ReadString(body, "nombre") ?? "").Trim();
            var percentage = ReadDouble(body, "porcentaje") ?? ReadDouble(body, "discountValue") ?? 0;

            if (code.Length < 2) return "Codigo de descuento invalido.";
            if (name.Length < 3) return "Ingrese un nombre de descuento valido.";
            if (percentage <= 0 || percentage > 100) return "El porcentaje debe estar entre 1 y 100.";
            return null;
        }

        protected static string? ValidateCompetencePayload(JsonElement body, bool requireFutureStart = false)
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

        protected async Task<string?> ValidateTeamPayload(JsonElement body, int? existingTeamId = null)
        {
            var name = (ReadString(body, "nombre") ?? ReadString(body, "name") ?? "").Trim();
            if (name.Length < 2) return "Nombre de equipo invalido.";

            var duplicate = await Context.Teams.AnyAsync(t => t.name == name && (!existingTeamId.HasValue || t.idTeam != existingTeamId.Value));
            return duplicate ? "Ya existe un equipo con ese nombre." : null;
        }

        protected async Task<int> ResolveProfessorProfileId(JsonElement body, int fallback = 0)
        {
            var id = ReadNestedInt(body, "profesor", "idProfessor") ?? ReadInt(body, "profesorId");
            if (id.HasValue && await Context.ProfessorProfiles.AnyAsync(p => p.idProfessor == id.Value)) return id.Value;

            var userId = ReadNestedInt(body, "profesor", "idUsuario") ?? ReadNestedInt(body, "profesor", "id") ?? ReadInt(body, "idProfesor");
            if (userId.HasValue)
            {
                var profileId = await Context.ProfessorProfiles
                    .Where(p => p.personalClubProfile.idUser == userId.Value)
                    .Select(p => p.idProfessor)
                    .FirstOrDefaultAsync();
                if (profileId != 0) return profileId;
            }

            return fallback != 0 ? fallback : await Context.ProfessorProfiles.Select(p => p.idProfessor).FirstOrDefaultAsync();
        }

        protected async Task<int> ResolveCourtId(JsonElement body, int fallback = 0)
        {
            var id = ReadInt(body, "canchaId") ?? ReadInt(body, "courtId") ?? ReadNestedInt(body, "cancha", "idCancha") ?? ReadNestedInt(body, "cancha", "id");
            if (id.HasValue) return id.Value;

            var name = ReadString(body, "cancha");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var courtId = await Context.Courts.Where(c => c.name == name).Select(c => c.idCourt).FirstOrDefaultAsync();
                if (courtId != 0) return courtId;
            }

            return fallback;
        }

        protected async Task<int> ClientProfileIdFromUser(int? userId)
        {
            if (!userId.HasValue) return 0;
            return await Context.ClientProfiles.Where(c => c.idUser == userId.Value).Select(c => c.idClient).FirstOrDefaultAsync();
        }

        protected async Task<object?> ProjectUser(int id)
        {
            var user = await Context.Users.Include(u => u.clientProfile).Include(u => u.personalClubProfile)!.ThenInclude(p => p!.employeeProfile).Include(u => u.personalClubProfile)!.ThenInclude(p => p!.professorProfile).FirstOrDefaultAsync(u => u.Id == id);
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

        protected async Task<IActionResult> UpdateUser(int id, JsonElement body)
        {
            var user = await Context.Users.Include(u => u.personalClubProfile).ThenInclude(p => p!.employeeProfile).Include(u => u.personalClubProfile).ThenInclude(p => p!.professorProfile).FirstOrDefaultAsync(u => u.Id == id);
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
            await UnitOfWork!.SaveChangesAsync();
            return Ok((await ProjectUser(id))!);
        }

        protected async Task<IActionResult> ToggleUser(int id)
        {
            var user = await Context.Users.FindAsync(id);
            if (user is null) return NotFound();
            user.isActive = !user.isActive;
            await UnitOfWork!.SaveChangesAsync();
            return Ok((await ProjectUser(id))!);
        }

        protected static User BuildUser(JsonElement body)
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

        protected async Task EnsureRole(User user, string role)
        {
            if (!await IdentityUserManager!.IsInRoleAsync(user, role))
            {
                await IdentityUserManager!.AddToRoleAsync(user, role);
            }
        }

        protected Disponibility BuildDisponibility(JsonElement body)
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

        protected object ProjectReserva(Reservation r)
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

        protected async Task<double> CalcularMontoApiContract(int courtId, TimeSpan start, TimeSpan end)
        {
            var court = await Context.Courts.Include(c => c.courtType).FirstOrDefaultAsync(c => c.idCourt == courtId);
            return court == null ? 0 : Math.Max(0, (end - start).TotalHours) * court.courtType.pricePerHour;
        }

        protected async Task<int> FirstClientId()
        {
            return await Context.ClientProfiles.Select(c => c.idClient).FirstOrDefaultAsync();
        }

        protected static int DurationByCourtType(string name)
        {
            var lower = name.ToLowerInvariant();
            if (lower.Contains("5")) return 60;
            if (lower.Contains("7")) return 90;
            return 120;
        }

        protected static string DayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miercoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sabado",
            _ => "Domingo"
        };

        protected static DayOfWeek? ParseDay(string? value)
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

        protected static TimeSpan HourToTime(double value)
        {
            var hours = (int)Math.Floor(value);
            var minutes = (int)Math.Round((value - hours) * 60);
            return new TimeSpan(hours, minutes, 0);
        }

        protected static string? ReadString(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
        }

        protected static int? ReadInt(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)) return value;
            return int.TryParse(property.ToString(), out value) ? value : null;
        }

        protected static double? ReadDouble(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)) return value;
            return double.TryParse(property.ToString(), out value) ? value : null;
        }

        protected static bool? ReadBool(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            if (property.ValueKind == JsonValueKind.True) return true;
            if (property.ValueKind == JsonValueKind.False) return false;
            return bool.TryParse(property.ToString(), out var value) ? value : null;
        }

        protected static DateTime? ReadDate(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return DateTime.TryParse(property.ToString(), out var value) ? value : null;
        }

        protected static TimeSpan? ReadTime(JsonElement body, string name)
        {
            if (!body.TryGetProperty(name, out var property)) return null;
            return TimeSpan.TryParse(property.ToString(), out var value) ? value : null;
        }

        protected static int? ReadNestedInt(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadInt(node, name) : null;
        }

        protected static string? ReadNestedString(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadString(node, name) : null;
        }

        protected static double? ReadNestedDouble(JsonElement body, string parent, string name)
        {
            return body.TryGetProperty(parent, out var node) ? ReadDouble(node, name) : null;
        }
    }
}





