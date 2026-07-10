using GolAhora.Data.UnitOfWork;
using GolAhora.Data;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services;

public class DiscountsService
{
    private readonly GolAhora.Data.AppContext _context;
        private readonly IUnitOfWork _unitOfWork;

    public DiscountsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
    }

    // RF50 --> Crear descuento
    public async Task<(bool success, string message)> AddDiscountAsync(DiscountDTO dto)
    {
        var exists = await _context.Discounts.AnyAsync(d => d.nombre == dto.Name);
        if (exists)
            return (false, "Ya existe un descuento con ese nombre.");

        var discount = new Discounts
        {
            nombre = dto.Name,
            discountType = dto.DiscountType,
            discountValue = dto.DiscountValue,
            conditions = dto.Conditions,
            startDate = dto.StartDate,
            endDate = dto.EndDate
        };

        _context.Discounts.Add(discount);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Descuento configurado y registrado exitosamente.");
    }

    // RF50 --> Modificar descuento
    public async Task<(bool success, string message)> UpdateDiscountAsync(int id, DiscountDTO dto)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if (discount == null)
            return (false, "Descuento no encontrado.");

        discount.nombre = dto.Name;
        discount.discountType = dto.DiscountType;
        discount.discountValue = dto.DiscountValue;
        discount.conditions = dto.Conditions;
        discount.startDate = dto.StartDate;
        discount.endDate = dto.EndDate;

        await _unitOfWork.SaveChangesAsync();
        return (true, "Descuento actualizado exitosamente.");
    }

    public async Task<DiscountDetailDTO?> GetDiscountByIdAsync(int id)
    {
        return await _context.Discounts
            .Where(d => d.idDiscount == id)
            .Select(d => new DiscountDetailDTO
            {
                IdDiscount = d.idDiscount,
                Name = d.nombre,
                DiscountType = d.discountType,
                DiscountValue = d.discountValue,
                Conditions = d.conditions,
                StartDate = d.startDate,
                EndDate = d.endDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<DiscountResponseDTO>> GetAllDiscountsAsync()
    {
        return await _context.Discounts
            .Select(d => new DiscountResponseDTO
            {
                IdDiscount = d.idDiscount,
                Name = d.nombre,
                DiscountType = d.discountType,
                DiscountValue = d.discountValue
            })
            .ToListAsync();
    }
}


