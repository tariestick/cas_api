using Casino_onlineAPI.modelos;
using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Casino_onlineAPI.servicios
{
    public class JuegoService : IJuegoService
    {
        private readonly IMongoCollection<Juego> _juegosCollection;

        public JuegoService(IMongoDatabase database)
        {
            _juegosCollection = database.GetCollection<Juego>("juegos");
        }

        public async Task<Result<Juego>> CrearJuego(Juego juego)
        {
            // Validar el modelo
            if (!IsValid(juego))
            {
                return Result<Juego>.Failure("Datos de juego no válidos.");
            }

            try
            {
                await _juegosCollection.InsertOneAsync(juego);
                return Result<Juego>.Success(juego);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al crear juego: {ex.Message}");
                return Result<Juego>.Failure("Ocurrió un error al crear el juego.");
            }
        }

        public async Task<Juego?> GetJuegoById(string id)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return null;
            }
            return await _juegosCollection.Find(j => j.Id == objectId.ToString()).FirstOrDefaultAsync();
        }

        public async Task<List<Juego>> GetAllJuegos()
        {
            return await _juegosCollection.Find(Builders<Juego>.Filter.Empty).ToListAsync();
        }

        public async Task<Result<Juego>> UpdateJuego(Juego juego)
        {
            if (!IsValid(juego))
            {
                return Result<Juego>.Failure("Datos de juego no válidos.");
            }

            ObjectId objectId;
            if (!ObjectId.TryParse(juego.Id, out objectId))
            {
                return Result<Juego>.Failure("Id de juego no válido.");
            }

            var filter = Builders<Juego>.Filter.Eq(j => j.Id, objectId.ToString());

            try
            {
                var result = await _juegosCollection.ReplaceOneAsync(filter, juego);
                if (result.ModifiedCount == 0)
                {
                    return Result<Juego>.Failure("No se encontró el juego o no hubo cambios.");
                }
                return Result<Juego>.Success(juego);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al actualizar juego: {ex.Message}");
                return Result<Juego>.Failure("Ocurrió un error al actualizar el juego.");
            }
        }

        public async Task<Result<bool>> DeleteJuego(string id)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return Result<bool>.Failure("Id de juego no válido.");
            }

            var filter = Builders<Juego>.Filter.Eq(j => j.Id, objectId.ToString());

            try
            {
                var result = await _juegosCollection.DeleteOneAsync(filter);
                if (result.DeletedCount == 0)
                {
                    return Result<bool>.Failure("No se encontró el juego.");
                }
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al eliminar juego: {ex.Message}");
                return Result<bool>.Failure("Ocurrió un error al eliminar el juego.");
            }
        }

        private bool IsValid(Juego juego)
        {
            var context = new ValidationContext(juego);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(juego, context, results, true);
        }
    }
}
