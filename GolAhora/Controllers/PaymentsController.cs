using GolAhora.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly Data.AppContext _context;

        public PaymentsController(Data.AppContext context)
        {
            _context = context;
        }

        // GET: api/payments --> RF48
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payments>>> GetPayments()
        {
            return await _context.Payments
                .Include(p => p.client)
                .Include(p => p.discount)
                .ToListAsync();
        }

        //GET: api/payments/id --> RF48
        [HttpGet("{id}")]
        public async Task<ActionResult<Payments>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.client)
                .Include(p => p.discount)
                .FirstOrDefaultAsync(p => p.idPayment == id);

            if (payment == null)
            {
                return NotFound(new { message = $"Pago con ID {id} no encontrado." });
            }

            return payment;
        }

        // GET: api/payments/id/print --> RF49
        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetPaymentTicket(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.client)
                .Include(p => p.discount)
                .FirstOrDefaultAsync(p => p.idPayment == id);

            if (payment == null)
            {
                return NotFound(new { message = $"Pago con ID {id} no encontrado para impresión." });
            }

            var ticketContent =
                "==================================================\n" +
                "                    GOL AHORA                     \n" +
                "               COMPROBANTE DE COBRO               \n" +
                "==================================================\n" +
                $"ID Pago: {payment.idPayment}\n" +
                $"Fecha/Hora: {payment.paymentDate:dd/MM/yyyy HH:mm}\n" +
                $"Método de Pago: {payment.paymentMethod}\n" +
                $"Estado de Transacción: {(payment.isSuccessful ? "Aprobado" : "Rechazado")}\n" +
                "--------------------------------------------------\n" +
                "DATOS DEL CLIENTE:\n" +
                $"ID Cliente: {payment.idClient}\n" +
                "--------------------------------------------------\n" +
                "DETALLE DEL COBRO:\n" +
                $"Monto Base: ${payment.amount}\n";

            if (payment.discount != null)
            {
                ticketContent += $"Descuento Aplicado: {payment.discount.nombre} (-${payment.discount.discountValue})\n";
            }

            ticketContent +=
                "--------------------------------------------------\n" +
                $"TOTAL ABONADO: ${payment.amount}\n" +
                "==================================================\n" +
                "       Gracias por su pago - Control Interno      \n" +
                "==================================================\n";

            var byteArray = System.Text.Encoding.UTF8.GetBytes(ticketContent);
            var stream = new System.IO.MemoryStream(byteArray);

            return File(stream, "text/plain", $"Ticket_Cobro_{id}.txt");
        }
    }
}
