using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : Controller
{
    private readonly DiscountsService _discountsService;

    public DiscountsController(DiscountsService discountsService)
    {
        _discountsService = discountsService; 
    }

    // POST: api/Discounts --> RF50
    [HttpPost]
    public async Task<ActionResult<Discounts>> CreateDiscount([FromBody] Discounts discount)
    {
        var createdDiscount = await _discountsService.CreateDiscountsAsync(discount);
        return CreatedAtAction(nameof(GetDiscountById), new { id = createdDiscount.idDiscount }, createdDiscount);
    }

    // GET: api/Discounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Discounts>> GetDiscountById(int id)
    {
        var discount = await _discountsService.GetDiscountByIdAsync(id);
        if(discount == null)
        {
            return NotFound(new { message = $"Descuento con ID {id} no encontrado." });
        }
        return Ok(discount);
    }

    // PUT: api/Discount/5 --> RF50
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discounts discountDetails)
    {
        if (id != discountDetails.idDiscount)
        {
            return BadRequest(new { message = "El ID del descuento no coincide." });
        }

        var updatedDiscount = await _discountsService.UpdateDiscountAsync(id, discountDetails);
        if (updatedDiscount == null)
        {
            return NotFound(new { message = $"Descuento con ID {id} no encontrado para actualizar." });
        }

        return Ok(new { message = "Descuento gestionado y actualizado con éxito.", data = updatedDiscount });
    }
}
