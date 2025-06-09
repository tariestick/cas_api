using Casino_onlineAPI.modelos;
using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Casino_onlineAPI.servicios
{
    public class PartidasService : IPartidasService
    {
        private readonly IMongoCollection<Partida> _partidasCollection;
        private readonly IMongoCollection<Usuario> _usuariosCollection; // Para actualizar el saldo del usuario
        private readonly IUsuariosService _usuariosService;
        private readonly IJuegoService _juegoService;

        public PartidasService(IMongoDatabase database , IUsuariosService usuariosService, IJuegoService juegoService)
        {
            _partidasCollection = database.GetCollection<Partida>("partidas");
            _usuariosCollection = database.GetCollection<Usuario>("usuarios");
            _usuariosService = usuariosService;
            _juegoService = juegoService;
        }

        public async Task<Result<Partida>> CrearPartida(Partida partida)
        {
            // Validar el modelo
            if (!IsValid(partida))
            {
                return Result<Partida>.Failure("Datos de partida no válidos.");
            }

            //Validar que el usuario existe
            var usuario = await _usuariosCollection.Find(u => u.Id == partida.UsuarioId).FirstOrDefaultAsync();
            if (usuario == null)
            {
                return Result<Partida>.Failure("El usuario no existe.");
            }

            //Validar que el juego existe (esto podría requerir un servicio de Juegos)
            var juegoResult = await _juegoService.GetJuegoById(partida.JuegoId);
            if (juegoResult == null)
            {
                return Result<Partida>.Failure("El juego no existe.");
            }


            partida.Fecha = DateTime.UtcNow; // Establecer la fecha de la partida

            try
            {
                await _partidasCollection.InsertOneAsync(partida);

               
                return Result<Partida>.Success(partida);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al crear partida: {ex.Message}");
                return Result<Partida>.Failure("Ocurrió un error al crear la partida.");
            }
        }

        public async Task<Partida?> GetPartidaById(string id)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return null;
            }

            return await _partidasCollection.Find(p => p.Id == objectId.ToString()).FirstOrDefaultAsync();
        }

        public async Task<List<Partida>> GetPartidasByUsuarioId(string usuarioId, int page = 1, int pageSize = 10)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(usuarioId, out objectId))
            {
                return new List<Partida>(); // o podrías lanzar una excepción BadRequestException
            }

            var filter = Builders<Partida>.Filter.Eq(p => p.UsuarioId, objectId.ToString());
            return await _partidasCollection.Find(filter)
                                            .Skip((page - 1) * pageSize)
                                            .Limit(pageSize)
                                            .ToListAsync();
        }

        public async Task<Result<Partida>> UpdatePartida(Partida partida)
        {
            if (!IsValid(partida))
            {
                return Result<Partida>.Failure("Datos de partida no válidos.");
            }
            ObjectId objectId;
            if (!ObjectId.TryParse(partida.Id, out objectId))
            {
                return Result<Partida>.Failure("Id de partida no válido.");
            }

            var filter = Builders<Partida>.Filter.Eq(x => x.Id, objectId.ToString());

            try
            {
                var result = await _partidasCollection.ReplaceOneAsync(filter, partida);
                if (result.ModifiedCount == 0)
                {
                    return Result<Partida>.Failure("No se encontró la partida o no hubo cambios.");
                }
                return Result<Partida>.Success(partida);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al actualizar partida: {ex.Message}");
                return Result<Partida>.Failure("Ocurrió un error al actualizar la partida.");
            }
        }

        private bool IsValid(Partida partida)
        {
            var context = new ValidationContext(partida);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(partida, context, results, true);
        }
    }
}
