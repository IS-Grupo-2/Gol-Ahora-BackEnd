using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GolAhora.Services;

public partial class PaymentsService : ServicePayloadBase
{
    private readonly Data.AppContext _context;
    private readonly IUnitOfWork _unitOfWork;

    protected override Data.AppContext Context => _context;
    protected override IUnitOfWork? UnitOfWork => _unitOfWork;

    public PaymentsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
            _context = unitOfWork.Context;
    }

    // RF44 – El sistema debe registrar los cobros de los servicios.
    public async Task<(bool success, string message)> RegistrarCobro(PaymentDTO dto)
    {
        var nuevoCobro = new Payments
        {
            idClient = dto.idClient,
            amount = dto.amount,
            paymentDate = dto.paymentDate,
            paymentMethod = dto.paymentMethod,
            isSuccessful = true,
            idDiscount = dto.idDiscount
        };

        _context.Payments.Add(nuevoCobro);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Cobro registrado exitosamente.");
    }

    // RF45 – El sistema debe modificar los cobros.
    public async Task<(bool success, string message)> ModificarCobro(int id, PaymentDTO dto)
    {
        var cobro = await _context.Payments.FindAsync(id);
        if (cobro == null)
            return (false, "Cobro no encontrado.");

        cobro.idClient = dto.idClient;
        cobro.amount = dto.amount;
        cobro.paymentDate = dto.paymentDate;
        cobro.paymentMethod = dto.paymentMethod;
        cobro.isSuccessful = dto.isSuccessful;
        cobro.idDiscount = dto.idDiscount;

        await _unitOfWork.SaveChangesAsync();
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

        await _unitOfWork.SaveChangesAsync();
        return (true, "Cobro dado de baja de manera exitosa.");
    }

    // CONSULTAR COBRO
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

    // CALCULAR MONTO TOTAL
    public async Task<double> CalcularMontoTotal()
    {
        return await _context.Payments
            .Where(p => p.isSuccessful)
            .SumAsync(p => p.amount);
    }

    // APLICAR DESCUENTO
    public async Task AplicarDescuento(int idPayment, Discounts descuento)
    {
        var cobro = await _context.Payments.FindAsync(idPayment);

        if (cobro != null && descuento != null)
        {
            cobro.idDiscount = descuento.idDiscount;
            cobro.amount = cobro.amount - descuento.discountValue;

            await _unitOfWork.SaveChangesAsync();
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

        var descuentoAplicado = payment.discount?.discountValue ?? 0;
        var montoOriginal = payment.amount + descuentoAplicado;
        var montoFinal = payment.amount;

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
            $"Monto Base: ${montoOriginal:F2}\n";

        if (payment.discount != null)
        {
            ticketContent += $"Descuento Aplicado: {payment.discount.nombre}\n";
            ticketContent += $"Valor Descuento: -${descuentoAplicado:F2}\n";
        }

        ticketContent +=
            "--------------------------------------------------\n" +
            $"TOTAL ABONADO: ${montoFinal:F2}\n" +
            "==================================================\n" +
            "       Gracias por su pago - Control Interno      \n" +
            "==================================================\n";

        return Encoding.UTF8.GetBytes(ticketContent);
    }
}



