using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : Controller
    {
        private readonly ReceiptsService _receiptsService;

        public ReceiptsController(ReceiptsService receiptsService)
        {
            _receiptsService = receiptsService;
        }

        // POST: api/Receipts --> RF51 Registrar recibo
        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] ReceiptDTO dto)
        {
            var result = await _receiptsService.AddReceiptAsync(dto);
            if (!result.success)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }
    }
}