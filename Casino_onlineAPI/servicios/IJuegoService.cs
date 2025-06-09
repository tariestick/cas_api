using Casino_onlineAPI.modelos;

namespace Casino_onlineAPI.servicios
{
    public interface IJuegoService
    {
        Task<Result<Juego>> CrearJuego(Juego juego);
        Task<Result<bool>> DeleteJuego(string id);
        Task<List<Juego>> GetAllJuegos();
        Task<Juego?> GetJuegoById(string id);
        Task<Result<Juego>> UpdateJuego(Juego juego);
    }
}