using System.ComponentModel.DataAnnotations;
using Casino_onlineAPI.modelos;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Casino_onlineAPI.servicios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly IMongoCollection<Usuario> _usuariosCollection;

        public UsuariosService(IMongoDatabase database)
        {
            _usuariosCollection = database.GetCollection<Usuario>("usuarios");
        }

        public async Task<Usuario?> GetUsuarioById(string id) // Cambiado a string? y Usuario?
        {
            // Primero, intenta convertir el string a un ObjectId.  Si falla, devuelve null.
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return null;
            }

            // Luego, realiza la búsqueda utilizando el ObjectId.
            return await _usuariosCollection.Find(u => u.Id == objectId.ToString()).FirstOrDefaultAsync(); //si el id es null va a explotar aqui
        }

        public async Task<Result<Usuario>> CrearUsuario(Usuario usuario)
        {
            // Validar el modelo (DataAnnotations)
            if (!IsValid(usuario)) // Usamos una función privada para la validación
            {
                return Result<Usuario>.Failure("Datos de usuario no válidos.");
            }

            // Verificar si el nombre de usuario ya existe (evitar duplicados)
            var existingUser = await _usuariosCollection.Find(u => u.NombreUsuario == usuario.NombreUsuario).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Result<Usuario>.Failure("El nombre de usuario ya existe.");
            }

            // Encriptar la contraseña usando BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            usuario.Contraseña = hashedPassword;

            try
            {
                await _usuariosCollection.InsertOneAsync(usuario);
                return Result<Usuario>.Success(usuario);
            }
            catch (Exception ex)
            {
                // Loguear el error (esto es muy importante para depuración y monitoreo)
                Console.Error.WriteLine($"Error al insertar usuario: {ex.Message}");
                return Result<Usuario>.Failure("Ocurrió un error al crear el usuario.");
            }
        }

        public async Task<Result<Usuario>> UpdateUsuarioCorreoYPassword(string idUsuario, string nuevoCorreo, string nuevaPassword)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(idUsuario, out objectId))
            {
                return Result<Usuario>.Failure("Id de usuario no válido.");
            }

            var filter = Builders<Usuario>.Filter.Eq(x => x.Id, objectId.ToString());

            // Construimos la actualización
            var updateDef = Builders<Usuario>.Update.Set(u => u.CorreoElectronico, nuevoCorreo);

            if (!string.IsNullOrEmpty(nuevaPassword))
            {
                // Hashear la contraseña antes de guardar
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                updateDef = updateDef.Set(u => u.Contraseña, hashedPassword);
            }

            try
            {
                var result = await _usuariosCollection.UpdateOneAsync(filter, updateDef);
                if (result.ModifiedCount == 0)
                {
                    return Result<Usuario>.Failure("No se encontró el usuario o no hubo cambios.");
                }

                var usuarioActualizado = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
                return Result<Usuario>.Success(usuarioActualizado);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al actualizar usuario: {ex.Message}");
                return Result<Usuario>.Failure("Ocurrió un error al actualizar el usuario.");
            }
        }

        public async Task<Result<bool>> DeleteUsuario(string id)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return Result<bool>.Failure("Id de usuario no válido.");
            }
            var filter = Builders<Usuario>.Filter.Eq(x => x.Id, objectId.ToString());
            try
            {
                var result = await _usuariosCollection.DeleteOneAsync(filter);
                if (result.DeletedCount == 0)
                {
                    return Result<bool>.Failure("No se encontró el usuario.");
                }
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al eliminar usuario: {ex.Message}");
                return Result<bool>.Failure("Ocurrió un error al eliminar el usuario.");
            }
        }

        public async Task<Usuario?> LoginUsuario(string nombreUsuario, string password)
        {
            try
            {
                // Buscar el usuario directamente en la colección por nombre de usuario
                var usuario = await _usuariosCollection
                    .Find(u => u.NombreUsuario == nombreUsuario)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return null; // Usuario no encontrado
                }

                // Verificar si la contraseña es correcta usando BCrypt
                bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(password, usuario.Contraseña);

                if (!passwordCorrecta)
                {
                    return null; // Contraseña incorrecta
                }

                // Devolver el usuario si la contraseña es correcta
                return usuario;
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción
                Console.Error.WriteLine($"Error en login: {ex.Message}");
                return null; // Error en caso de excepción
            }
        }



        public async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            return await _usuariosCollection.Find(u => u.CorreoElectronico == email).FirstOrDefaultAsync();
        }



        private bool IsValid(Usuario usuario)
        {
            var context = new ValidationContext(usuario);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(usuario, context, results, true);
        }
        public async Task<Result<Usuario>> UpdateUserBalance(string userId, decimal amount)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(userId, out objectId))
            {
                return Result<Usuario>.Failure("Id de usuario no válido.");
            }

            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, objectId.ToString());
            var update = Builders<Usuario>.Update.Inc(u => u.Saldo, amount);

            try
            {
                var result = await _usuariosCollection.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0)
                {
                    return Result<Usuario>.Failure("No se pudo actualizar el saldo del usuario.");
                }

                var updatedUser = await _usuariosCollection.Find(filter).FirstOrDefaultAsync(); //obtener el usuario actualizado
                return Result<Usuario>.Success(updatedUser);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al actualizar saldo del usuario: {ex.Message}");
                return Result<Usuario>.Failure("Ocurrió un error al actualizar el saldo del usuario.");
            }
        }
    }
}
