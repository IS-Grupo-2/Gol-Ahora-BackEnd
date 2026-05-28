using GolAhora.Models;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Services;

public class DiscountsService
{
    private readonly Data.AppContext _context;

    public DiscountsService(Data.AppContext context)
    {
        _context = context;
    }

    // RF50 --> crear dto.
    public async Task<Discounts> CreateDiscountsAsync(Discounts discount)
    {
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();
        return discount;
    }

    public async Task<Discounts?> GetDiscountByIdAsync(int id)
    {
        return await _context.Discounts.FindAsync(id);
    }

    //RF50 --> Modificar dto.
    public async Task<Discounts?> UpdateDiscountAsync(int id, Discounts discountDetails)
    {
        var existingDiscount = await _context.Discounts.FindAsync(id);
        if (existingDiscount == null) return null;

        // Mapeamos los campos reales que encontramos en Payments.cs
        existingDiscount.nombre = discountDetails.nombre;
        existingDiscount.discountType = discountDetails.discountType;
        existingDiscount.discountValue = discountDetails.discountValue;
        existingDiscount.conditions = discountDetails.conditions;
        existingDiscount.startDate = discountDetails.startDate;
        existingDiscount.endDate = discountDetails.endDate;

        await _context.SaveChangesAsync();
        return existingDiscount;
    }
}
