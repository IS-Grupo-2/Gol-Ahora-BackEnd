using System.Text.Json;
using GolAhora.Data.UnitOfWork;
using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{    public partial class PaymentsService
    {

        public async Task<IActionResult> GetCobros()
        {
            var pagos = await _context.Payments
                .Include(p => p.client).ThenInclude(c => c.user)
                .Include(p => p.discount)
                .Include(p => p.reservation)
                .ToListAsync();
            return Ok(pagos.Select(p => new
            {
                idCobro = p.idPayment,
                idReserva = p.reservation?.idReservation,
                cliente = new { idUsuario = p.client.idUser, idClient = p.idClient, nombre = p.client.user.name, apellido = p.client.user.lastName, dni = p.client.user.DNI },
                concepto = p.reservation == null ? "Cobro" : $"Reserva #{p.reservation.idReservation}",
                tipoCobro = p.reservation == null ? "Servicio" : "Reserva Cancha",
                monto = p.amount + (p.discount?.discountValue ?? 0),
                descuento = p.discount?.nombre,
                montoFinal = p.amount,
                fecha = p.paymentDate.ToString("yyyy-MM-dd"),
                estado = p.isSuccessful ? "pagado" : "pendiente",
                metodo = p.paymentMethod
            }));
        }

        public async Task<IActionResult> CreateCobro([FromBody] JsonElement body)
        {
            var validation = await ValidatePaymentPayload(body);
            if (validation is not null) return ValidationError(validation);

            var pago = new Payments
            {
                idClient = ReadNestedInt(body, "cliente", "idClient") ?? ReadNestedInt(body, "cliente", "idCliente") ?? ReadInt(body, "idClient") ?? 0,
                amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? 0,
                paymentDate = ReadDate(body, "fecha") ?? DateTime.UtcNow,
                paymentMethod = ReadString(body, "metodo") ?? "No informado",
                isSuccessful = ReadString(body, "estado") == "pagado",
                idDiscount = ReadInt(body, "idDiscount")
            };
            _context.Payments.Add(pago);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = pago.idPayment });
        }

        public async Task<IActionResult> UpdateCobro(int id, [FromBody] JsonElement body)
        {
            var pago = await _context.Payments.FindAsync(id);
            if (pago is null) return NotFound();
            var validation = await ValidatePaymentPayload(body, existingClientId: pago.idClient);
            if (validation is not null) return ValidationError(validation);

            pago.amount = ReadDouble(body, "montoFinal") ?? ReadDouble(body, "monto") ?? pago.amount;
            pago.paymentMethod = ReadString(body, "metodo") ?? pago.paymentMethod;
            pago.isSuccessful = ReadString(body, "estado") == "pagado";
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = id });
        }

        public async Task<IActionResult> DeleteCobro(int id)
        {
            var pago = await _context.Payments.FindAsync(id);
            if (pago is null) return NotFound();
            pago.isSuccessful = false;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idCobro = id });
        }

        public async Task<IActionResult> GetDescuentos()
        {
            var descuentos = await _context.Discounts.ToListAsync();
            return Ok(descuentos.Select(d => new
            {
                id = d.idDiscount,
                codigo = d.discountType,
                nombre = d.nombre,
                porcentaje = d.discountValue,
                descripcion = d.conditions,
                activo = d.startDate <= DateTime.UtcNow && d.endDate >= DateTime.UtcNow
            }));
        }

        public async Task<IActionResult> CreateDescuento([FromBody] JsonElement body)
        {
            var validation = ValidateDiscountPayload(body);
            if (validation is not null) return ValidationError(validation);

            var descuento = new Discounts
            {
                nombre = ReadString(body, "nombre") ?? "",
                discountType = ReadString(body, "codigo") ?? ReadString(body, "discountType") ?? "",
                discountValue = ReadDouble(body, "porcentaje") ?? ReadDouble(body, "discountValue") ?? 0,
                conditions = ReadString(body, "descripcion") ?? ReadString(body, "conditions") ?? "",
                startDate = DateTime.UtcNow,
                endDate = ReadBool(body, "activo") == false ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddYears(1)
            };
            _context.Discounts.Add(descuento);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id = descuento.idDiscount });
        }

        public async Task<IActionResult> UpdateDescuento(int id, [FromBody] JsonElement body)
        {
            var descuento = await _context.Discounts.FindAsync(id);
            if (descuento is null) return NotFound();
            var validation = ValidateDiscountPayload(body);
            if (validation is not null) return ValidationError(validation);

            descuento.nombre = ReadString(body, "nombre") ?? descuento.nombre;
            descuento.discountType = ReadString(body, "codigo") ?? descuento.discountType;
            descuento.discountValue = ReadDouble(body, "porcentaje") ?? descuento.discountValue;
            descuento.conditions = ReadString(body, "descripcion") ?? descuento.conditions;
            descuento.endDate = ReadBool(body, "activo") == false ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddYears(1);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        public async Task<IActionResult> DeleteDescuento(int id)
        {
            var descuento = await _context.Discounts.FindAsync(id);
            if (descuento is null) return NotFound();
            descuento.endDate = DateTime.UtcNow.AddDays(-1);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { id });
        }

        public async Task<IActionResult> GetRecibos()
        {
            var recibos = await _context.Receipts.Include(r => r.payment).ThenInclude(p => p.client).ThenInclude(c => c.user).ToListAsync();
            return Ok(recibos.Select(r => new
            {
                idRecibo = r.idReceipt,
                nroRecibo = r.receiptNumber,
                cobro = new { idCobro = r.idPayment, montoFinal = r.payment.amount },
                cliente = new { idUsuario = r.payment.client.idUser, nombre = r.payment.client.user.name, apellido = r.payment.client.user.lastName, dni = r.payment.client.user.DNI },
                pago = new { metodoPago = r.payment.paymentMethod, fechaPago = r.payment.paymentDate, estado = r.payment.isSuccessful ? "Completado" : "Pendiente" },
                fecha = r.date.ToString("yyyy-MM-dd"),
                total = r.totalAmount,
                detalles = r.details,
                estado = "emitido"
            }));
        }

        public async Task<IActionResult> CreateRecibo([FromBody] JsonElement body)
        {
            var paymentId = ReadNestedInt(body, "cobro", "idCobro") ?? ReadInt(body, "idPayment") ?? 0;
            if (!await _context.Payments.AnyAsync(p => p.idPayment == paymentId)) return ValidationError("Cobro inexistente.");
            var total = ReadDouble(body, "total") ?? ReadNestedDouble(body, "cobro", "montoFinal") ?? 0;
            if (total < 0) return ValidationError("El total del recibo no puede ser negativo.");

            var recibo = new Receipts
            {
                idPayment = paymentId,
                receiptNumber = ReadString(body, "nroRecibo") ?? $"0001-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100000000:00000000}",
                totalAmount = ReadDouble(body, "total") ?? ReadNestedDouble(body, "cobro", "montoFinal") ?? 0,
                details = ReadString(body, "detalles") ?? "",
                date = ReadDate(body, "fecha") ?? DateTime.UtcNow
            };
            _context.Receipts.Add(recibo);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = recibo.idReceipt });
        }

        public async Task<IActionResult> UpdateRecibo(int id, [FromBody] JsonElement body)
        {
            var recibo = await _context.Receipts.FindAsync(id);
            if (recibo is null) return NotFound();
            recibo.details = ReadString(body, "detalles") ?? recibo.details;
            recibo.totalAmount = ReadDouble(body, "total") ?? recibo.totalAmount;
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = id });
        }

        public async Task<IActionResult> DeleteRecibo(int id)
        {
            var recibo = await _context.Receipts.FindAsync(id);
            if (recibo is null) return NotFound();
            _context.Receipts.Remove(recibo);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { idRecibo = id });
        }
    }
}



