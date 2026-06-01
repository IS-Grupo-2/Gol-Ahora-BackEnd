using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        public ReservationsController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // RF19 + RF24 – POST api/reservations
        [HttpPost]
        public async Task<IActionResult> AgregarReservation([FromBody] ReservationDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la reserva son inválidos.");

            var (success, message) = await _reservationService.AgregarReservation(dto);

            if (!success)
            {
                if (message.Contains("no existe") || message.Contains("no encontrada"))
                    return NotFound(new { mensaje = message });
                return BadRequest(new { mensaje = message });
            }

            return Ok(new { mensaje = message });
        }

        // RF20a – PUT api/reservations/{id}/horario
        [HttpPut("{id}/horario")]
        public async Task<IActionResult> ModificarHorario(int id, [FromBody] CambiarHorarioDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del horario son inválidos.");

            var (success, message) = await _reservationService.ModificarHorario(id, dto);

            if (!success)
            {
                if (message.Contains("no encontrada"))
                    return NotFound(new { mensaje = message });
                return BadRequest(new { mensaje = message });
            }

            return Ok(new { mensaje = message });
        }

        // RF20b – PUT api/reservations/{id}/fecha
        [HttpPut("{id}/fecha")]
        public async Task<IActionResult> ModificarFecha(int id, [FromBody] CambiarFechaDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos de la fecha son inválidos.");

            var (success, message) = await _reservationService.ModificarFecha(id, dto);

            if (!success)
            {
                if (message.Contains("no encontrada"))
                    return NotFound(new { mensaje = message });
                return BadRequest(new { mensaje = message });
            }

            return Ok(new { mensaje = message });
        }

        // RF21 – GET api/reservations
        [HttpGet]
        public async Task<IActionResult> ListarReservations()
        {
            var reservations = await _reservationService.ListarReservations();
            return Ok(reservations);
        }

        // RF22 + RF25 + RF26 – DELETE api/reservations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarReservation(int id)
        {
            var (success, message, montoFinal) = await _reservationService.EliminarReservation(id);

            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message, montoFinal = montoFinal });
        }

        // RF23 – GET api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarReservation(int id)
        {
            var reservation = await _reservationService.ConsultarReservation(id);

            if (reservation == null)
                return NotFound($"No se encontró la reserva con ID {id}.");

            return Ok(reservation);
        }
    }
}
