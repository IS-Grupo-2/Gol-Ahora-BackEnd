using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class CourtService
    {

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

        public async Task<IActionResult> DeleteTipoCancha(int id)
        {
            var type = await _context.CourtTypes.Include(t => t.courts).FirstOrDefaultAsync(t => t.idTypeCourt == id);
            if (type is null) return NotFound();
            foreach (var court in type.courts) court.isAvailable = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

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

        public async Task<IActionResult> ToggleCancha(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court is null) return NotFound();
            court.isAvailable = !court.isAvailable;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = court.idCourt, activa = court.isAvailable, estado = court.isAvailable ? "activa" : "inactiva" });
        }

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

        public async Task<IActionResult> CreateDisponibilidad([FromBody] JsonElement body)
        {
            var disp = BuildDisponibility(body);
            var validation = await ValidateDisponibility(disp);
            if (validation is not null) return ValidationError(validation);

            _context.Disponibilities.Add(disp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = disp.idDisponibility });
        }

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

        public async Task<IActionResult> DeleteDisponibilidad(int id)
        {
            var disp = await _context.Disponibilities.FindAsync(id);
            if (disp is null) return NotFound();
            _context.Disponibilities.Remove(disp);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idDisponibilidad = id });
        }
    }
}



