using GolAhora.Data;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class ReceiptsService
    {
        private readonly GolAhora.Data.AppContext _context;

        public ReceiptsService(GolAhora.Data.AppContext context)
        {
            _context = context;
        }

        // RF51 --> Registrar recibo de pago
        public async Task<(bool success, string message)> AddReceiptAsync(ReceiptDTO dto)
        {
            var paymentExists = await _context.Payments.AnyAsync(p => p.idPayment == dto.IdPayment);
            if (!paymentExists)
                return (false, $"No se puede registrar el recibo. El ID de Pago {dto.IdPayment} no existe.");

            var receipt = new Receipts
            {
                idPayment = dto.IdPayment,
                receiptNumber = dto.ReceiptNumber,
                totalAmount = dto.TotalAmount,
                details = dto.Details,
                date = dto.Date ?? DateTime.Now 
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();
            return (true, "Recibo registrado exitosamente.");
        }

        // RF52 --> Modificar recibo de pago
        public async Task<(bool success, string message)> UpdateReceiptAsync(int id, ReceiptDTO dto)
        {
            var existingReceipt = await _context.Receipts.FindAsync(id);
            if(existingReceipt == null)
            {
                return (false, "Recibo no encontrado para modificar.");
            }

            var paymentExists = await _context.Payments.AnyAsync(p => p.idPayment == dto.IdPayment);
            if (!paymentExists)
                return (false, $"No se puede modificar. El ID de pago {dto.IdPayment} no existe.");

            existingReceipt.idPayment = dto.IdPayment;
            existingReceipt.receiptNumber = dto.ReceiptNumber;
            existingReceipt.totalAmount = dto.TotalAmount;
            existingReceipt.details = dto.Details;

            if (dto.Date.HasValue)
            {
                existingReceipt.date = dto.Date.Value;
            }

            await _context.SaveChangesAsync();
            return (true, "Recibo modificado exitosamente");
        }

        // RF53 --> Generar un listado de recibos
        public async Task<List<ReceiptResponseDTO>> GetAllReceiptsAsync()
        {
            return await _context.Receipts
                .Select(r => new ReceiptResponseDTO
                {
                    IdReceipt = r.idReceipt,
                    ReceiptNumber = r.receiptNumber,
                    TotalAmount = r.totalAmount,
                    Date = r.date
                })
                .ToListAsync();
        }

        // RF54 --> Dar de baja a los recibos de pagos
        public async Task<(bool success, string message)> DeleteReceiptAsync(int id)
        {
            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt == null)
                return (false, "Recibo no encontrado para dar de baja.");

            if (receipt.details.StartsWith("ANULADO -"))
                return (false, "Este recibo ya fue dado de baja previamente.");

            receipt.details = "ANULADO - " + receipt.details;

            await _context.SaveChangesAsync();
            return (true, "Recibo dado de baja exitosamente.");
        }
    }
}