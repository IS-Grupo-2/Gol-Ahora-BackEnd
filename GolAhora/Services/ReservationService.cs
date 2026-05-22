using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ReservationService
    {
        private readonly GolAhora.Data.AppContext _context;

        public ReservationService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF19 + RF24 + RF17 + RS01 + RS02
        public async Task<(bool success, string message)> AgregarReservation(ReservationDTO dto)
        {
            // RS01 – no más de 30 días de antelación
            if ((dto.reservationDate - DateTime.Now).TotalDays > 30)
                return (false, "No se pueden realizar reservas con más de 30 días de antelación.");

            var cancha = await _context.Courts
                .Include(c => c.courtType)
                .FirstOrDefaultAsync(c => c.idCourt == dto.idCourt);

            if (cancha == null)
                return (false, "La cancha no existe.");

            // Validar que la cancha esté activa
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

            // RS02 + RF17 – verificar disponibilidad y que no esté bloqueada
            var disponible = await _context.Disponibilities.AnyAsync(d =>
                d.courtId == dto.idCourt &&
                d.day == dto.reservationDate.DayOfWeek &&
                d.startTime <= dto.startTime &&
                d.endTime >= dto.endTime &&
                d.isAvailable
            );

            if (!disponible)
                return (false, "La cancha no está disponible en ese horario.");

            var nuevaReservation = new Reservation
            {
                idClient = dto.idClient,
                idCourt = dto.idCourt,
                reservationDate = dto.reservationDate,
                startTime = dto.startTime,
                endTime = dto.endTime,
                totalPrice = dto.totalPrice,
                idPayment = dto.idPayment ?? 0,
                isPaid = false
            };

            // RF24 – confirmar si el pago ya está registrado y validado
            if (dto.idPayment != null && dto.idPayment != 0)
            {
                var pago = await _context.Payments.FindAsync(dto.idPayment);
                if (pago != null && pago.isSuccessful && pago.amount >= dto.totalPrice)
                    nuevaReservation.isPaid = true;
            }

            _context.Reservations.Add(nuevaReservation);
            await _context.SaveChangesAsync();
            return (true, "Reserva registrada exitosamente.");
        }

        // RF20 – Modificar una reserva existente
        public async Task<bool> ModificarReservation(int id, ReservationDTO dto)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return false;

            reservation.idClient = dto.idClient;
            reservation.idCourt = dto.idCourt;
            reservation.reservationDate = dto.reservationDate;
            reservation.startTime = dto.startTime;
            reservation.endTime = dto.endTime;
            reservation.totalPrice = dto.totalPrice;
            reservation.idPayment = dto.idPayment ?? 0;

            await _context.SaveChangesAsync();
            return true;
        }

        // RF21 – Listar todas las reservas
        public async Task<List<ReservationResponseDTO>> ListarReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.client)
                .Include(r => r.court)
                .ToListAsync();

            return reservations.Select(r => new ReservationResponseDTO
            {
                idReservation = r.idReservation,
                idClient = r.idClient,
                clienteNombre = r.client.user.name,
                clienteApellido = r.client.user.name,
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

        // RF22 + RF25 + RF26 – Cancelar reserva con validación de antelación y reembolso
        public async Task<(bool success, string message)> EliminarReservation(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.idReservation == id);

            if (reservation == null)
                return (false, "Reserva no encontrada.");

            var antelacion = reservation.reservationDate - DateTime.Now;

            if (antelacion.TotalHours < 48)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                return (true, "Reserva cancelada. Por cancelar fuera del plazo de 48 horas se aplica un cargo y no corresponde reembolso.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return (true, "Reserva cancelada. Se procesará un reembolso total del pago.");
        }

        // RF23 – Consultar una reserva por ID
        public async Task<ReservationResponseDTO?> ConsultarReservation(int id)
        {
            var r = await _context.Reservations
                .Include(r => r.client)
                .Include(r => r.court)
                .FirstOrDefaultAsync(r => r.idReservation == id);

            if (r == null) return null;

            return new ReservationResponseDTO
            {
                idReservation = r.idReservation,
                idClient = r.idClient,
                clienteNombre = r.client.user.name,
                clienteApellido = r.client.user.name,
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
        public async Task<(bool success, string message, double monto)> CalcularMonto(int idCourt, TimeSpan startTime, TimeSpan endTime)
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
