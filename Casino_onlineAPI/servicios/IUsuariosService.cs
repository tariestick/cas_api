using Casino_onlineAPI.modelos;

namespace Casino_onlineAPI.servicios
{
    public interface IUsuariosService
    {
        Task<Result<Usuario>> CrearUsuario(Usuario usuario);
        Task<Usuario?> GetUsuarioById(string id);

        Task<Result<Usuario>> UpdateUsuarioCorreoYPassword(string idUsuario, string nuevoCorreo, string nuevaPassword);


        Task<Result<bool>> DeleteUsuario(string id);
        Task<Result<Usuario>> UpdateUserBalance(string userId, decimal amount);
        Task<Usuario?> LoginUsuario(string nombreUsuario, string password);
        Task<Usuario?> GetUsuarioByEmail(string email);

    }
}