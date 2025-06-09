using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Casino_onlineAPI.modelos; 
using System.Threading.Tasks;
using BCrypt.Net;
using Casino_onlineAPI.config;
using Casino_onlineAPI.servicios;

namespace Casino_onlineAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMongoCollection<Usuario> _usuariosCollection;
        private readonly IUsuariosService _usuariosService;


        public UsuariosController(MongoDBService mongoDBService , IUsuariosService usuariosService)
        {
            _usuariosCollection = mongoDBService.Database?.GetCollection<Usuario>("usuarios");
            _usuariosService = usuariosService;
        }

        [HttpGet]
        public async Task<IEnumerable<Usuario>> Get()
        {
            return await _usuariosCollection.Find(FilterDefinition<Usuario>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Usuario>> GetById(string id)
        {
            var usuario = await _usuariosService.GetUsuarioById(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost]

        public async Task<ActionResult> Post(Usuario usuario)
        {
            var result = await _usuariosService.CrearUsuario(usuario);
            if (result.IsSuccess)
            {
                // El CreatedAtAction usa nameof para evitar errores de tipeo y mejorar el mantenimiento
                return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value); // Aquí Value ya es Usuario
            }
            else
            {
                if (result.ErrorMessage == "Datos de usuario no válidos.")
                {
                    return BadRequest(ModelState); // Devuelve los errores de validación estándar
                }
                else if (result.ErrorMessage == "El nombre de usuario ya existe.")
                {
                    return Conflict(new { mensaje = result.ErrorMessage });
                }
                else
                {
                    return StatusCode(500, new { mensaje = result.ErrorMessage });
                }
            }
        }






        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            if (usuarioDto == null)
            {
                return BadRequest("Datos no válidos.");
            }

            var result = await _usuariosService.UpdateUsuarioCorreoYPassword(id, usuarioDto.CorreoElectronico, usuarioDto.Password);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.ErrorMessage == "Id de usuario no válido.")
            {
                return BadRequest(result.ErrorMessage);
            }
            else if (result.ErrorMessage == "No se encontró el usuario o no hubo cambios.")
            {
                return NotFound(result.ErrorMessage);
            }
            else
            {
                return StatusCode(500, new { mensaje = result.ErrorMessage });
            }
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> Delete(string id)
        {
            var result = await _usuariosService.DeleteUsuario(id);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else if (result.ErrorMessage == "Id de usuario no válido.")
            {
                return BadRequest(result.ErrorMessage);
            }
            else
            {
                return NotFound(result.ErrorMessage);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var usuario = await _usuariosService.LoginUsuario(loginRequest.NombreUsuario, loginRequest.Password);

            if (usuario == null)
            {
                // Respuesta con objeto JSON y código 401
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            // Respuesta con usuario serializado como JSON
            return Ok(new
            {
                mensaje = "Inicio de sesión correcto",
                usuario = usuario
            });
        }

        [HttpGet("me")]
        public async Task<ActionResult<Usuario>> GetCurrentUser([FromHeader(Name = "X-User-Id")] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Falta el ID del usuario en la cabecera.");

            var usuario = await _usuariosService.GetUsuarioById(userId);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            return Ok(new
            {
                usuario.NombreUsuario,
                usuario.Saldo
            });
        }

        [HttpPut("actualizar-saldo/{idUsuario}")]
        public async Task<IActionResult> ActualizarSaldo(string idUsuario, [FromBody] decimal nuevoSaldo)
        {
            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, idUsuario);
            var update = Builders<Usuario>.Update.Set(u => u.Saldo, nuevoSaldo);

            var resultado = await _usuariosCollection.UpdateOneAsync(filter, update);

            if (resultado.MatchedCount == 0)
                return NotFound();

            var usuarioActualizado = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
            return Ok(usuarioActualizado);
        }







    }
}























        