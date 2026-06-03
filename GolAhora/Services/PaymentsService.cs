using GolAhora.DTOs;
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
    // RF44 – El sistema debe registrar los cobros de los servicios.MILENA
    public async Task<(bool success, string message)> RegistrarCobro(PaymentDTO dto)
    {
        var nuevoCobro = new Payments
        {
            idClient = dto.idClient,
            amount = dto.amount,
            paymentDate = dto.paymentDate,
            paymentMethod = dto.paymentMethod,
            isSuccessful = true, // Al registrarse arranca como exitoso
            idDiscount = dto.idDiscount
        };

        _context.Payments.Add(nuevoCobro);
        await _context.SaveChangesAsync();

        return (true, "Cobro registrado exitosamente.");
    }

    // RF45 – El sistema debe modificar los cobros.
    public async Task<(bool success, string message)> ModificarCobro(int id, PaymentDTO dto)
    {
        var cobro = await _context.Payments.FindAsync(id);
        if (cobro == null)
            return (false, "Cobro no encontrado.");

        // Actualizamos los valores con los que vienen en el DTO
        cobro.idClient = dto.idClient;
        cobro.amount = dto.amount;
        cobro.paymentDate = dto.paymentDate;
        cobro.paymentMethod = dto.paymentMethod;
        cobro.isSuccessful = dto.isSuccessful;
        cobro.idDiscount = dto.idDiscount;

        await _context.SaveChangesAsync();
        return (true, "Cobro modificado exitosamente.");
    }

    // RF46 – El sistema debe generar un listado de cobros básico.
    public async Task<List<PaymentResponseDTO>> ListarCobros()
    {
        return await _context.Payments
            .Select(p => new PaymentResponseDTO
            {
                idPayment = p.idPayment,
                idClient = p.idClient,
                amount = p.amount,
                paymentDate = p.paymentDate,
                paymentMethod = p.paymentMethod,
                isSuccessful = p.isSuccessful
            })
            .ToListAsync();
    }

    // RF47 – El sistema debe dar de baja a los cobros (Baja lógica).
    public async Task<(bool success, string message)> DarDeBajaCobro(int id)
    {
        var cobro = await _context.Payments.FindAsync(id);
        if (cobro == null)
            return (false, "Cobro no encontrado.");

        cobro.isSuccessful = false;

        await _context.SaveChangesAsync();
        return (true, "Cobro dado de baja de manera exitosa.");
    }

    // CONSULTAR COBRO (Trae el detalle de un cobro específico por su ID)
    public async Task<PaymentDetailDTO?> ConsultarCobro(int id)
    {
        return await _context.Payments
            .Where(p => p.idPayment == id)
            .Select(p => new PaymentDetailDTO
            {
                idPayment = p.idPayment,
                idClient = p.idClient,
                amount = p.amount,
                paymentDate = p.paymentDate,
                paymentMethod = p.paymentMethod,
                isSuccessful = p.isSuccessful,
                idDiscount = p.idDiscount
            })
            .FirstOrDefaultAsync();
    }

    // CALCULAR MONTO TOTAL (Suma todos los cobros exitosos del sistema)
    public async Task<double> CalcularMontoTotal()
    {
        return await _context.Payments
            .Where(p => p.isSuccessful)
            .SumAsync(p => p.amount);
    }

    // APLICAR DESCUENTO (Recibe el ID del cobro y el objeto descuento)
    public async Task AplicarDescuento(int idPayment, Discounts descuento)
    {
        var cobro = await _context.Payments.FindAsync(idPayment);

        if (cobro != null && descuento != null)
        {
            cobro.idDiscount = descuento.idDiscount;
            cobro.amount = cobro.amount - descuento.discountValue;

            await _context.SaveChangesAsync();
        }
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
