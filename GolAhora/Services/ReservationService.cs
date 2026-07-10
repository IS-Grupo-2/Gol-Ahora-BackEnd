using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ReservationService
    {
        private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
        }

        // RF19 + RF24 + RF17 + RS01 + RS02
        public async Task<(bool success, string message)> AgregarReservation(ReservationDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Validar fecha no pasada
                    if (dto.reservationDate.Date < DateTime.Now.Date)
                        return (false, "No se puede reservar para una fecha pasada.");

                    // RS01 – no más de 30 días de antelación
                    if ((dto.reservationDate - DateTime.Now).TotalDays > 30)
                        return (false, "No se pueden realizar reservas con más de 30 días de antelación.");

                    var cancha = await _context.Courts
                        .Include(c => c.courtType)
                        .FirstOrDefaultAsync(c => c.idCourt == dto.idCourt);

                    if (cancha == null)
                        return (false, "La cancha no existe.");

                    if (!cancha.isAvailable)
                        return (false, "La cancha no está disponible.");

                    var duracion = (dto.endTime - dto.startTime).TotalHours;

                    var nombre = cancha.courtType.name.ToLower();

                    var maxHoras = nombre.Contains("5") ? 1.0
                                 : nombre.Contains("7") ? 1.5
                                 : nombre.Contains("11") ? 2.0
                                 : 2.0;

                    if (duracion > maxHoras)
                        return (false, $"La duración máxima para este tipo de cancha es {maxHoras} horas.");

                    var disponible = await _context.Disponibilities.AnyAsync(d =>
                        d.courtId == dto.idCourt &&
                        d.day == dto.reservationDate.DayOfWeek &&
                        d.startTime <= dto.startTime &&
                        d.endTime >= dto.endTime &&
                        d.isAvailable
                    );

                    if (!disponible)
                        return (false, "Ese horario está fuera del horario habilitado de la cancha.");

                    var superpuesta = await _context.Reservations.AnyAsync(r =>
                        r.idCourt == dto.idCourt &&
                        r.reservationDate.Date == dto.reservationDate.Date &&
                        r.startTime < dto.endTime &&
                        r.endTime > dto.startTime
                    );

                    if (superpuesta)
                        return (false, "Ya existe una reserva en ese horario para esa cancha.");

                    var totalPrice = duracion * cancha.courtType.pricePerHour;

                    // VALIDACIÓN DEL PAGO
                    if (dto.idPayment == null || dto.idPayment == 0)
                        return (false, "Debe proporcionar un pago válido.");

                    var pago = await _context.Payments.FindAsync(dto.idPayment);

                    if (pago == null)
                        return (false, "El pago no existe.");

                    if (!pago.isSuccessful)
                        return (false, "El pago no fue aprobado.");

                    if (pago.amount < totalPrice)
                        return (false, "El monto del pago es insuficiente.");

                    // Validar que el pago no esté siendo usado por otra reserva
                    var pagoEnUso = await _context.Reservations.AnyAsync(r =>
                        r.idPayment == dto.idPayment
                    );

                    if (pagoEnUso)
                        return (false, "Este pago ya está siendo utilizado por otra reserva.");

                    // Usar el monto del pago (que YA tiene descuento aplicado si existe)
                    var nuevaReservation = new Reservation
                    {
                        idClient = dto.idClient,
                        idCourt = dto.idCourt,
                        reservationDate = dto.reservationDate,
                        startTime = dto.startTime,
                        endTime = dto.endTime,
                        totalPrice = pago.amount,  // ? Usa el amount del pago (con descuento)
                        idPayment = dto.idPayment.Value,
                        isPaid = true
                    };

                    _context.Reservations.Add(nuevaReservation);
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, $"Reserva registrada exitosamente. Total: {pago.amount:C}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al registrar la reserva: {ex.Message}");
                }
            }
        }

        // RF20a – Modificar SOLO horario de una reserva pagada
        public async Task<(bool success, string message)> ModificarHorario(int id, CambiarHorarioDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.court)
                        .ThenInclude(c => c.courtType)
                        .FirstOrDefaultAsync(r => r.idReservation == id);

                    if (reservation == null)
                        return (false, "Reserva no encontrada.");

                    if (!reservation.isPaid)
                        return (false, "Solo se pueden modificar reservas pagadas.");

                    // Validar >= 6 horas antes
                    var antelacion = reservation.reservationDate.Date + reservation.startTime - DateTime.Now;
                    if (antelacion.TotalHours < 6)
                        return (false, "No se puede modificar la reserva con menos de 6 horas de anticipación.");

                    var duracion = (dto.endTime - dto.startTime).TotalHours;
                    var nombre = reservation.court.courtType.name.ToLower();

                    var maxHoras = nombre.Contains("5") ? 1.0
                                 : nombre.Contains("7") ? 1.5
                                 : nombre.Contains("11") ? 2.0
                                 : 2.0;

                    if (duracion > maxHoras)
                        return (false, $"La duración máxima para este tipo de cancha es {maxHoras} horas.");

                    var disponible = await _context.Disponibilities.AnyAsync(d =>
                        d.courtId == reservation.idCourt &&
                        d.day == reservation.reservationDate.DayOfWeek &&
                        d.startTime <= dto.startTime &&
                        d.endTime >= dto.endTime &&
                        d.isAvailable
                    );

                    if (!disponible)
                        return (false, "Ese horario no está disponible.");

                    var superpuesta = await _context.Reservations.AnyAsync(r =>
                        r.idReservation != id &&
                        r.idCourt == reservation.idCourt &&
                        r.reservationDate.Date == reservation.reservationDate.Date &&
                        r.startTime < dto.endTime &&
                        r.endTime > dto.startTime
                    );

                    if (superpuesta)
                        return (false, "Ya existe una reserva en ese horario.");

                    reservation.startTime = dto.startTime;
                    reservation.endTime = dto.endTime;

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Horario modificado exitosamente.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al modificar el horario: {ex.Message}");
                }
            }
        }

        // RF20b – Modificar SOLO fecha de una reserva pagada
        public async Task<(bool success, string message)> ModificarFecha(int id, CambiarFechaDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.court)
                        .ThenInclude(c => c.courtType)
                        .FirstOrDefaultAsync(r => r.idReservation == id);

                    if (reservation == null)
                        return (false, "Reserva no encontrada.");

                    if (!reservation.isPaid)
                        return (false, "Solo se pueden modificar reservas pagadas.");

                    // Validar >= 6 horas antes
                    var antelacion = reservation.reservationDate.Date + reservation.startTime - DateTime.Now;
                    if (antelacion.TotalHours < 6)
                        return (false, "No se puede modificar la reserva con menos de 6 horas de anticipación.");

                    // Validar fecha no pasada
                    if (dto.reservationDate.Date < DateTime.Now.Date)
                        return (false, "No se puede modificar a una fecha pasada.");

                    if ((dto.reservationDate - DateTime.Now).TotalDays > 30)
                        return (false, "No se pueden modificar a más de 30 días de antelación.");

                    var disponible = await _context.Disponibilities.AnyAsync(d =>
                        d.courtId == reservation.idCourt &&
                        d.day == dto.reservationDate.DayOfWeek &&
                        d.startTime <= reservation.startTime &&
                        d.endTime >= reservation.endTime &&
                        d.isAvailable
                    );

                    if (!disponible)
                        return (false, "Ese horario no está disponible en la nueva fecha.");

                    var superpuesta = await _context.Reservations.AnyAsync(r =>
                        r.idReservation != id &&
                        r.idCourt == reservation.idCourt &&
                        r.reservationDate.Date == dto.reservationDate.Date &&
                        r.startTime < reservation.endTime &&
                        r.endTime > reservation.startTime
                    );

                    if (superpuesta)
                        return (false, "Ya existe una reserva en ese horario para esa fecha.");

                    reservation.reservationDate = dto.reservationDate;

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Fecha modificada exitosamente.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al modificar la fecha: {ex.Message}");
                }
            }
        }

        // RF20c – Modificar FECHA Y HORARIO de una reserva pagada
        public async Task<(bool success, string message)> ModificarAmbos(int id, CambiarFechaYHorarioDTO dto)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.court)
                        .ThenInclude(c => c.courtType)
                        .FirstOrDefaultAsync(r => r.idReservation == id);

                    if (reservation == null)
                        return (false, "Reserva no encontrada.");

                    if (!reservation.isPaid)
                        return (false, "Solo se pueden modificar reservas pagadas.");

                    // Validar >= 6 horas antes
                    var antelacion = reservation.reservationDate.Date + reservation.startTime - DateTime.Now;
                    if (antelacion.TotalHours < 6)
                        return (false, "No se puede modificar la reserva con menos de 6 horas de anticipación.");

                    // Validar fecha no pasada
                    if (dto.reservationDate.Date < DateTime.Now.Date)
                        return (false, "No se puede modificar a una fecha pasada.");

                    // Validar 30 días
                    if ((dto.reservationDate - DateTime.Now).TotalDays > 30)
                        return (false, "No se pueden modificar a más de 30 días de antelación.");

                    var duracion = (dto.endTime - dto.startTime).TotalHours;
                    var nombre = reservation.court.courtType.name.ToLower();

                    var maxHoras = nombre.Contains("5") ? 1.0
                                 : nombre.Contains("7") ? 1.5
                                 : nombre.Contains("11") ? 2.0
                                 : 2.0;

                    if (duracion > maxHoras)
                        return (false, $"La duración máxima para este tipo de cancha es {maxHoras} horas.");

                    var disponible = await _context.Disponibilities.AnyAsync(d =>
                        d.courtId == reservation.idCourt &&
                        d.day == dto.reservationDate.DayOfWeek &&
                        d.startTime <= dto.startTime &&
                        d.endTime >= dto.endTime &&
                        d.isAvailable
                    );

                    if (!disponible)
                        return (false, "Ese horario no está disponible en la nueva fecha.");

                    var superpuesta = await _context.Reservations.AnyAsync(r =>
                        r.idReservation != id &&
                        r.idCourt == reservation.idCourt &&
                        r.reservationDate.Date == dto.reservationDate.Date &&
                        r.startTime < dto.endTime &&
                        r.endTime > dto.startTime
                    );

                    if (superpuesta)
                        return (false, "Ya existe una reserva en ese horario para esa fecha.");

                    reservation.reservationDate = dto.reservationDate;
                    reservation.startTime = dto.startTime;
                    reservation.endTime = dto.endTime;

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Fecha y horario modificados exitosamente.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al modificar la reserva: {ex.Message}");
                }
            }
        }

        // RF21 – Listar todas las reservas
        public async Task<List<ReservationResponseDTO>> ListarReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.client)
                    .ThenInclude(c => c.user)
                .Include(r => r.court)
                .ToListAsync();

            return reservations.Select(r => new ReservationResponseDTO
            {
                idReservation = r.idReservation,
                idClient = r.idClient,
                clienteNombre = r.client.user.name,
                clienteApellido = r.client.user.lastName,
                idCourt = r.idCourt,
                canchaNombre = r.court.name,
                reservationDate = r.reservationDate,
                startTime = r.startTime,
                endTime = r.endTime,
                isPaid = r.isPaid,
                totalPrice = r.totalPrice,
                idPayment = r.idPayment
            }).ToList();
        }

        // RF22 + RF25 + RF26 – Cancelar reserva con validación de antelación y cargo por penalidad
        public async Task<(bool success, string message, double montoFinal)> EliminarReservation(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.idReservation == id);

            if (reservation == null)
                return (false, "Reserva no encontrada.", 0);

            var antelacion = reservation.reservationDate.Date + reservation.startTime - DateTime.Now;

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM Reservations WHERE idReservation = {0}", id);

            if (antelacion.TotalHours < 6)
            {
                var cargo = reservation.totalPrice * 0.5;
                var montoFinal = reservation.totalPrice - cargo;

                return (
                    true,
                    $"Reserva cancelada. Se aplicó un cargo del 50% (total: {reservation.totalPrice:C}) por cancelar con menos de 6 horas de anticipación.",
                    montoFinal
                );
            }

            return (
                true,
                $"Reserva cancelada. Se procesará un reembolso total del pago.",
                reservation.totalPrice
            );
        }

        // RF23 – Consultar una reserva por ID
        public async Task<ReservationResponseDTO?> ConsultarReservation(int id)
        {
            var r = await _context.Reservations
                .Include(r => r.client)
                    .ThenInclude(c => c.user)
                .Include(r => r.court)
                .FirstOrDefaultAsync(r => r.idReservation == id);

            if (r == null)
                return null;

            return new ReservationResponseDTO
            {
                idReservation = r.idReservation,
                idClient = r.idClient,
                clienteNombre = r.client.user.name,
                clienteApellido = r.client.user.lastName,
                idCourt = r.idCourt,
                canchaNombre = r.court.name,
                reservationDate = r.reservationDate,
                startTime = r.startTime,
                endTime = r.endTime,
                isPaid = r.isPaid,
                totalPrice = r.totalPrice,
                idPayment = r.idPayment
            };
        }

        // CalcularMonto – calcula el monto total en base a duración y precio por hora
        public async Task<(bool success, string message, double monto)> CalcularMonto(
            int idCourt,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            var cancha = await _context.Courts
                .Include(c => c.courtType)
                .FirstOrDefaultAsync(c => c.idCourt == idCourt);

            if (cancha == null)
                return (false, "La cancha no existe.", 0);

            var duracionHoras = (endTime - startTime).TotalHours;

            if (duracionHoras <= 0)
                return (false, "El horario de fin debe ser posterior al horario de inicio.", 0);

            var monto = duracionHoras * cancha.courtType.pricePerHour;

            return (true, $"Monto calculado: {monto:C}", monto);
        }
    }
}


