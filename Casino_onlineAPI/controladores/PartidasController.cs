using Casino_onlineAPI.modelos;
using Casino_onlineAPI.servicios;
using Microsoft.AspNetCore.Mvc;

namespace Casino_onlineAPI.controladores
{
    [ApiController]
    [Route("api/partidas")]
    public class PartidasController : ControllerBase
    {
        private readonly IPartidasService _partidasService;

        public PartidasController(IPartidasService partidasService)
        {
            _partidasService = partidasService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Partida partida)
        {
            var result = await _partidasService.CrearPartida(partida);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
            }
            else
            {
                return BadRequest(result.ErrorMessage); // O podrías usar diferentes códigos de estado para diferentes errores.
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Partida>> GetById(string id)
        {
            var partida = await _partidasService.GetPartidaById(id);
            if (partida == null)
            {
                return NotFound();
            }
            return Ok(partida);
        }

        [HttpGet("usuarios/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Partida>>> GetPartidasByUsuarioId(string usuarioId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var partidas = await _partidasService.GetPartidasByUsuarioId(usuarioId, page, pageSize);
            return Ok(partidas);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Partida partida)
        {
            var result = await _partidasService.UpdatePartida(partida);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.ErrorMessage == "Id de partida no válido.")
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
