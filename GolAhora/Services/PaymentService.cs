using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GolAhora.Models;
using GolAhora.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Services
{
    public class PaymentService
    {
        private readonly GolAhora.Data.AppContext? _context;

        public PaymentService(GolAhora.Data.AppContext context)
        {
            _context = context;
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
                isSuccessful = true, // Al registrarse arranca como exitoso
                idDiscount = dto.idDiscount
            };

            _context!.Payments.Add(nuevoCobro);
            await _context.SaveChangesAsync();

            return (true, "Cobro registrado exitosamente.");
        }

        // RF45 – El sistema debe modificar los cobros.
        public async Task<(bool success, string message)> ModificarCobro(int id, PaymentDTO dto)
        {
            var cobro = await _context!.Payments.FindAsync(id);
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

        // RF46 – El sistema debe generar un listado de cobros.
        public async Task<List<PaymentResponseDTO>> ListarCobros()
        {
            return await _context!.Payments
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

        // RF47 – El sistema debe dar de baja a los cobros (Baja lógica usando isSuccessful).
        public async Task<(bool success, string message)> DarDeBajaCobro(int id)
        {
            var cobro = await _context!.Payments.FindAsync(id);
            if (cobro == null)
                return (false, "Cobro no encontrado.");

            // En vez de borrar físicamente el registro (baja física), cambiamos su estado a false (baja lógica)
            cobro.isSuccessful = false;

            await _context.SaveChangesAsync();
            return (true, "Cobro dado de baja de manera exitosa.");
        }
        // 1. CONSULTAR COBRO (Trae el detalle de un cobro específico por su ID)
        public async Task<PaymentDetailDTO?> ConsultarCobro(int id)
        {
            return await _context!.Payments
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


        //segun UML
        //IMPRIMIR COBRO (Busca los datos del cobro para mandarlos a imprimir)
        public async Task<PaymentDetailDTO?> ImprimirCobro(int id)
        {
            // Buscamos el cobro igual que en la consulta
            var cobro = await ConsultarCobro(id);
            if (cobro == null) return null;

            // Acá el servicio retorna el objeto listo para que el Frontend genere el PDF/Ticket REVISAR
            return cobro;
        }

        //CALCULAR MONTO TOTAL (Suma todos los cobros exitosos del sistema)
        public async Task<double> CalcularMontoTotal()
        {
            // Suma el campo 'amount' de todos los cobros que fueron exitosos (isSuccessful == true)
            return await _context!.Payments
                .Where(p => p.isSuccessful)
                .SumAsync(p => p.amount);
        }

        // 4. APLICAR DESCUENTO (Recibe el ID del cobro y el objeto descuento, aplica la rebaja al monto)
        public async Task AplicarDescuento(int idPayment, Discounts descuento)
        {
            // Buscamos el cobro al que le queremos aplicar el descuento
            var cobro = await _context!.Payments.FindAsync(idPayment);

            if (cobro != null && descuento != null)
            {
                // Asignamos el ID del descuento al cobro
                cobro.idDiscount = descuento.idDiscount;

                // Restamos el valor del descuento al monto total del cobro
                cobro.amount = cobro.amount - descuento.discountValue;

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
        }

    }
}
