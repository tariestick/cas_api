using Casino_onlineAPI.modelos;
using Casino_onlineAPI.servicios;
using Microsoft.AspNetCore.Mvc;

namespace Casino_onlineAPI.controladores
{
    [ApiController]
    [Route("api/transacciones")]
    public class TransaccionController : ControllerBase
    {
        private readonly ITransaccionService _transaccionService;

        public TransaccionController(ITransaccionService transaccionService)
        {
            _transaccionService = transaccionService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Transaccion transaccion)
        {
            var result = await _transaccionService.CrearTransaccion(transaccion);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaccion>> GetById(string id)
        {
            var transaccion = await _transaccionService.GetTransaccionById(id);
            if (transaccion == null)
            {
                return NotFound();
            }
            return Ok(transaccion);
        }

        [HttpGet("usuarios/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Transaccion>>> GetByUsuarioId(string usuarioId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var transacciones = await _transaccionService.GetTransaccionesByUsuarioId(usuarioId, page, pageSize);
            return Ok(transacciones);
        }

        public class CreatePaymentIntentRequest
        {
            public decimal Amount { get; set; }
            public string UsuarioId { get; set; } // We'll expect the userId from the frontend
        }

        [HttpPost("create-payment-intent")] 
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "El monto debe ser mayor que cero." });
            }
            if (string.IsNullOrEmpty(request.UsuarioId))
            {
                return BadRequest(new { error = "El ID de usuario es requerido." });
            }

            // Call the service method to create the Payment Intent
            var result = await _transaccionService.CreatePaymentIntent(request.Amount, request.UsuarioId);

            if (result.IsSuccess)
            {
                // Return the client_secret to the frontend
                return Ok(new { clientSecret = result.Value });
            }
            else
            {
                // Handle errors from the service layer
                return StatusCode(500, new { error = result.ErrorMessage });
            }
        }
    }
}
