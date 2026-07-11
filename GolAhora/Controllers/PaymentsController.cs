using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly PaymentsService _paymentsService;

        public PaymentsController(PaymentsService paymentsService)
        {
            _paymentsService = paymentsService;
        }
         

        [HttpPost]
        public async Task<IActionResult> RegistrarCobro([FromBody] PaymentDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cobro son inválidos.");

            var (success, message) = await _paymentsService.RegistrarCobro(dto);
            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF45 – PUT api/payments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarCobro(int id, [FromBody] PaymentDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cobro son inválidos.");

            var (success, message) = await _paymentsService.ModificarCobro(id, dto);
            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF46 – GET api/payments/listar-basico
        [HttpGet("listar-basico")]
        public async Task<IActionResult> ListarCobros()
        {
            var lista = await _paymentsService.ListarCobros();
            return Ok(lista);
        }

        // RF47 – DELETE api/payments/{id} (Baja lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBajaCobro(int id)
        {
            var (success, message) = await _paymentsService.DarDeBajaCobro(id);
            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // GET api/payments/consultar/{id} (consulta )
        [HttpGet("consultar/{id}")]
        public async Task<IActionResult> ConsultarCobro(int id)
        {
            var cobro = await _paymentsService.ConsultarCobro(id);
            if (cobro == null)
                return NotFound($"No se encontró el cobro con ID {id}.");

            return Ok(cobro);
        }

        // GET api/payments/monto-total
        [HttpGet("monto-total")]
        public async Task<IActionResult> CalcularMontoTotal()
        {
            var total = await _paymentsService.CalcularMontoTotal();
            return Ok(new { montoTotalRecaudado = total });
        }

        // POST api/payments/aplicar-descuento/{idPayment}
        [HttpPost("aplicar-descuento/{idPayment}")]
        public async Task<IActionResult> AplicarDescuento(int idPayment, [FromBody] Discounts descuento)
        {
            if (descuento == null)
                return BadRequest("Los datos del descuento son inválidos.");

            await _paymentsService.AplicarDescuento(idPayment, descuento);
            return Ok(new { mensaje = "Descuento aplicado correctamente y monto actualizado." });
        }



        // GET: api/payments --> RF48
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payments>>> GetPayments()
        {
            var payments = await _paymentsService.GetPaymentsAsync();
            return Ok(payments);
        }

        //GET: api/payments/id --> RF48
        [HttpGet("{id}")]
        public async Task<ActionResult<Payments>> GetPayment(int id)
        {
            var payment = await _paymentsService.GetPaymentByIdAsync(id);
            if(payment == null)
            {
                return NotFound(new { message = $"Pago con ID {id} no encontrado." });
            }
            return Ok(payment);
        }

        // GET: api/payments/id/print --> RF49
        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetPaymentTicket(int id)
        {
            var ticketBytes = await _paymentsService.GeneratePaymentTicketAsync(id);
            if(ticketBytes == null)
            {
                return NotFound(new { message = $"Pago con ID {id} no encontrado para impresión." });
            }

            var stream = new System.IO.MemoryStream(ticketBytes);
            return File(stream, "text/plain", $"Ticket_Cobro_{id}.txt");
        }
        [HttpGet("/api/cobros")]
        public Task<IActionResult> GetCobrosApiContract() => _paymentsService.GetCobros();

        [HttpPost("/api/cobros")]
        public Task<IActionResult> CreateCobroApiContract([FromBody] JsonElement body) => _paymentsService.CreateCobro(body);

        [HttpPut("/api/cobros/{id}")]
        public Task<IActionResult> UpdateCobroApiContract(int id, [FromBody] JsonElement body) => _paymentsService.UpdateCobro(id, body);

        [HttpDelete("/api/cobros/{id}")]
        public Task<IActionResult> DeleteCobroApiContract(int id) => _paymentsService.DeleteCobro(id);
    }
}








