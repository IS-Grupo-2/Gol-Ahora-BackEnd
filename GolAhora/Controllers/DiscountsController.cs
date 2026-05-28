using GolAhora.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : Controller
{
    private readonly Data.AppContext _context;

    public DiscountsController(Data.AppContext context)
    {
        _context = context;
    }

    // POST: api/Discounts --> RF50
    [HttpPost]
    public async Task<ActionResult<Discounts>> CreateDiscount([FromBody] Discounts discount)
    {
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDiscountById), new { id = discount.idDiscount }, discount);
    }

    // GET: api/Discounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Discounts>> GetDiscountById(int id)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if(discount == null)
        {
            return NotFound(new { message = $"Descuento con ID {id} no encontrado." });
        }

        return discount;
    }

    // PUT: api/Discount/5 --> RF50
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discounts discountDetails)
    {
        if(id != discountDetails.idDiscount)
        {
            return BadRequest(new { message = "El ID del descuento no coincide. " });
        }

        var existingDiscount = await _context.Discounts.FindAsync(id);
        if (existingDiscount == null)
        {
            return NotFound(new { message = $"Descuento con ID {id} no encontrado" });
        }

        existingDiscount.nombre = discountDetails.nombre;
        existingDiscount.discountType = discountDetails.discountType;
        existingDiscount.discountValue = discountDetails.discountValue;
        existingDiscount.conditions = discountDetails.conditions;
        existingDiscount.startDate = discountDetails.startDate;
        existingDiscount.endDate = discountDetails.endDate;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Discounts.AnyAsync(d => d.idDiscount == id))
            {
                return NotFound();
            }
            throw;
        }
        return Ok(new { message = "Descuento gestionado y actualizado con éxito. ", data = existingDiscount });
    }
}
