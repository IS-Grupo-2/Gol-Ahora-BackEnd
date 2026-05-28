using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> CreateDiscount([FromBody] DiscountDTO dto)
    {
        var result = await _discountsService.AddDiscountAsync(dto);
        if (!result.success)
            return BadRequest(new { message = result.message });

        return Ok(new { message = result.message });
    }

    // PUT: api/Discounts/5 --> RF50
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] DiscountDTO dto)
    {
        var result = await _discountsService.UpdateDiscountAsync(id, dto);
        if (!result.success)
            return NotFound(new { message = result.message });

        return Ok(new { message = result.message });
    }

    // GET: api/Discounts/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscountById(int id)
    {
        var discount = await _discountsService.GetDiscountByIdAsync(id);
        if (discount == null)
            return NotFound(new { message = $"Descuento con ID {id} no encontrado." });

        return Ok(discount);
    }

    // GET: api/Discounts
    [HttpGet]
    public async Task<IActionResult> GetAllDiscounts()
    {
        var list = await _discountsService.GetAllDiscountsAsync();
        return Ok(list);
    }
}