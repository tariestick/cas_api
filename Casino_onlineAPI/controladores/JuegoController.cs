using Casino_onlineAPI.modelos;
using Casino_onlineAPI.servicios;
using Microsoft.AspNetCore.Mvc;

namespace Casino_onlineAPI.controladores
{
    [ApiController]
    [Route("api/juegos")]
    public class JuegoController : ControllerBase
    {
        private readonly IJuegoService _juegoService;

        public JuegoController(IJuegoService juegoService)
        {
            _juegoService = juegoService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Juego juego)
        {
            var result = await _juegoService.CrearJuego(juego);
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
        public async Task<ActionResult<Juego>> GetById(string id)
        {
            var juego = await _juegoService.GetJuegoById(id);
            if (juego == null)
            {
                return NotFound();
            }
            return Ok(juego);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Juego>>> GetAll()
        {
            var juegos = await _juegoService.GetAllJuegos();
            return Ok(juegos);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Juego juego)
        {
            var result = await _juegoService.UpdateJuego(juego);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.ErrorMessage == "Id de juego no válido.")
            {
                return BadRequest(result.ErrorMessage);
            }
            else
            {
                return NotFound(result.ErrorMessage);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var result = await _juegoService.DeleteJuego(id);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else if (result.ErrorMessage == "Id de juego no válido.")
            {
                return BadRequest(result.ErrorMessage);
            }
            else
            {
                return NotFound(result.ErrorMessage);
            }
        }
    }
}
