using Casino_onlineAPI.modelos;

namespace Casino_onlineAPI.servicios
{
    public interface ITransaccionService
    {
        Task<Result<Transaccion>> CrearTransaccion(Transaccion transaccion);
        Task<Transaccion?> GetTransaccionById(string id);
        Task<List<Transaccion>> GetTransaccionesByUsuarioId(string usuarioId, int page, int pageSize);
        Task<Result<string>> CreatePaymentIntent(decimal amount, string userId);
    }
}
