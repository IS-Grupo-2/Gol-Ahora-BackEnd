using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
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
    }
}
