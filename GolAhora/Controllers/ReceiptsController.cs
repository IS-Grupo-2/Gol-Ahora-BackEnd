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

        // GET: api/Receipts --> RF53 Generar listado de recibos
        [HttpGet]
        public async Task<IActionResult> GetAllReceipts()
        {
            var list = await _receiptsService.GetAllReceiptsAsync();
            return Ok(list);
        }

        // DELETE: api/Receipts/5 --> RF54 Dar de baja recibo
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var result = await _receiptsService.DeleteReceiptAsync(id);
            if (!result.success)
            {
                if (result.message.Contains("no encontrado"))
                    return NotFound(new { message = result.message });

                return BadRequest(new { message = result.message });
            }

            return Ok(new { message = result.message });
        }

        // GET: api/Receipts/{id} --> RF55 Consultar recibo
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceiptById(int id)
        {
            var receipt = await _receiptsService.GetReceiptByIdAsync(id);
            if (receipt == null)
                return NotFound(new { message = $"Recibo con ID {id} no encontrado." });
            return Ok(receipt);
        }

        // GET: api/Receipts/{id}/imprimir --> RF56 Imprimir recibo
        [HttpGet("{id}/imprimir")]
        public async Task<IActionResult> ImprimirReceipt(int id)
        {
            var receipt = await _receiptsService.ImprimirReceiptAsync(id);
            if (receipt == null)
                return NotFound(new { message = $"Recibo con ID {id} no encontrado." });
            return Ok(receipt);
        }
    }
}