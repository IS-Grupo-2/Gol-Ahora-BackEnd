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

        // PUT: api/Receipts/5 --> RF52 Modificar recibo
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceipt(int id, [FromBody] ReceiptDTO dto)
        {
            var result = await _receiptsService.UpdateReceiptAsync(id, dto);
            if (!result.success)
            {
                if (result.message.Contains("no encontrado"))
                    return NotFound(new { message = result.message });

                return BadRequest(new { message = result.message });
            }

            return Ok(new { message = result.message });
        }
    }
}