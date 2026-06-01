using GolAhora.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GolAhora.Services;

public class PaymentsService
{
    private readonly Data.AppContext _context;

    public PaymentsService(Data.AppContext context)
    {
        _context = context;
    }

    // RF48 --> Lista completa de pagos
    public async Task<IEnumerable<Payments>> GetPaymentsAsync()
    {
        return await _context.Payments
            .Include(p => p.client)
            .Include(p => p.discount)
            .ToListAsync();
    }

    // RF48 --> Cobro puntual por id
    public async Task<Payments?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.client)
            .Include(p => p.discount)
            .FirstOrDefaultAsync(p => p.idPayment == id);
    }

    // RF49 --> Cadena de bytes para ticket
    public async Task<byte[]?> GeneratePaymentTicketAsync(int id)
    {
        var payment = await GetPaymentByIdAsync(id);
        if (payment == null) return null;

        var ticketContent =
            "==================================================\n" +
            "                    GOL AHORA                     \n" +
            "               COMPROBANTE DE COBRO               \n" +
            "==================================================\n" +
            $"ID Pago: {payment.idPayment}\n" +
            $"Fecha/Hora: {payment.paymentDate:dd/MM/yyyy HH:mm}\n" +
            $"Método de Pago: {payment.paymentMethod}\n" +
            $"Estado de Transacción: {(payment.isSuccessful ? "Aprobado" : "Rechazado")}\n" +
            "--------------------------------------------------\n" +
            "DATOS DEL CLIENTE:\n" +
            $"ID Cliente: {payment.idClient}\n" +
            "--------------------------------------------------\n" +
            "DETALLE DEL COBRO:\n" +
            $"Monto Base: ${payment.amount}\n";

        if (payment.discount != null)
        {
            ticketContent += $"Descuento Aplicado: {payment.discount.nombre} (-${payment.discount.discountValue})\n";
        }

        ticketContent +=
            "--------------------------------------------------\n" +
            $"TOTAL ABONADO: ${payment.amount}\n" +
            "==================================================\n" +
            "       Gracias por su pago - Control Interno      \n" +
            "==================================================\n";

        return Encoding.UTF8.GetBytes(ticketContent);
    }
}
