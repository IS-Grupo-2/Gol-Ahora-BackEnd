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
    }
}
