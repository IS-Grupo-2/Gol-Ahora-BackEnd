using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GolAhora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : Controller
{
    private readonly DiscountsService _discountsService;
    private readonly PaymentsService _paymentsService;

    public DiscountsController(DiscountsService discountsService, PaymentsService paymentsService)
    {
        _discountsService = discountsService;
        _paymentsService = paymentsService;
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
    [HttpGet("/api/descuentos")]
    public Task<IActionResult> GetDescuentosApiContract() => _paymentsService.GetDescuentos();

    [HttpPost("/api/descuentos")]
    public Task<IActionResult> CreateDescuentoApiContract([FromBody] JsonElement body) => _paymentsService.CreateDescuento(body);

    [HttpPut("/api/descuentos/{id}")]
    public Task<IActionResult> UpdateDescuentoApiContract(int id, [FromBody] JsonElement body) => _paymentsService.UpdateDescuento(id, body);

    [HttpDelete("/api/descuentos/{id}")]
    public Task<IActionResult> DeleteDescuentoApiContract(int id) => _paymentsService.DeleteDescuento(id);}



