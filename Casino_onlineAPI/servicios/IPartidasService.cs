using Casino_onlineAPI.modelos;

namespace Casino_onlineAPI.servicios
{
    public interface IPartidasService
    {
        Task<Result<Partida>> CrearPartida(Partida partida);
        Task<Partida?> GetPartidaById(string id);
        Task<List<Partida>> GetPartidasByUsuarioId(string usuarioId, int page = 1, int pageSize = 10);
        Task<Result<Partida>> UpdatePartida(Partida partida);
    }
}