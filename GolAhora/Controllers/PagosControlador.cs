using GolAhora.DTOs;
using GolAhora.Models;
using GolAhora.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosControlador : ControllerBase
    {
        private readonly PaymentService _paymentService;

        // Inyectamos tu PaymentService en el constructor
        public PagosControlador(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // RF44 – POST api/pagoscontrolador
        [HttpPost]
        public async Task<IActionResult> RegistrarCobro([FromBody] PaymentDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cobro son inválidos.");

            var (success, message) = await _paymentService.RegistrarCobro(dto);
            if (!success)
                return BadRequest(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF45 – PUT api/pagoscontrolador/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarCobro(int id, [FromBody] PaymentDTO dto)
        {
            if (dto == null)
                return BadRequest("Los datos del cobro son inválidos.");

            var (success, message) = await _paymentService.ModificarCobro(id, dto);
            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // RF46 – GET api/pagoscontrolador
        [HttpGet]
        public async Task<IActionResult> ListarCobros()
        {
            var lista = await _paymentService.ListarCobros();
            return Ok(lista);
        }

        // RF47 – DELETE api/pagoscontrolador/{id} (Baja lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBajaCobro(int id)
        {
            var (success, message) = await _paymentService.DarDeBajaCobro(id);
            if (!success)
                return NotFound(new { mensaje = message });

            return Ok(new { mensaje = message });
        }

        // GET api/pagoscontrolador/consultar/{id}
        [HttpGet("consultar/{id}")]
        public async Task<IActionResult> ConsultarCobro(int id)
        {
            var cobro = await _paymentService.ConsultarCobro(id);
            if (cobro == null)
                return NotFound($"No se encontró el cobro con ID {id}.");

            return Ok(cobro);
        }

        // GET api/pagoscontrolador/imprimir/{id}
        [HttpGet("imprimir/{id}")]
        public async Task<IActionResult> ImprimirCobro(int id)
        {
            var ticket = await _paymentService.ImprimirCobro(id);
            if (ticket == null)
                return NotFound($"No se puede imprimir. No se encontró el cobro con ID {id}.");

            return Ok(ticket);
        }

        // GET api/pagoscontrolador/monto-total
        [HttpGet("monto-total")]
        public async Task<IActionResult> CalcularMontoTotal()
        {
            var total = await _paymentService.CalcularMontoTotal();
            return Ok(new { montoTotalRecaudado = total });
        }

        // POST api/pagoscontrolador/aplicar-descuento/{idPayment}
        [HttpPost("aplicar-descuento/{idPayment}")]
        public async Task<IActionResult> AplicarDescuento(int idPayment, [FromBody] Discounts descuento)
        {
            if (descuento == null)
                return BadRequest("Los datos del descuento son inválidos.");

            await _paymentService.AplicarDescuento(idPayment, descuento);
            return Ok(new { mensaje = "Descuento aplicado correctamente y monto actualizado." });
        }
    }
}
