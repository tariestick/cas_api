using Casino_onlineAPI.modelos;
using MongoDB.Bson;
using MongoDB.Driver;
using Stripe;
using System.ComponentModel.DataAnnotations;

namespace Casino_onlineAPI.servicios
{
    public class TransaccionService : ITransaccionService
    {
        private readonly IMongoCollection<Transaccion> _transaccionCollection;
        private readonly IUsuariosService _usuariosService;

        public TransaccionService(IMongoDatabase database, IUsuariosService usuariosService)
        {
            _transaccionCollection = database.GetCollection<Transaccion>("Transacciones");
            _usuariosService = usuariosService;
        }


        public async Task<Result<string>> CreatePaymentIntent(decimal amount, string userId)
        {
            if (amount <= 0)
            {
                return Result<string>.Failure("El monto debe ser mayor que cero.");
            }

            // Stripe expects amount in the lowest currency unit (e.g., cents for EUR)
            long stripeAmount = (long)(amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = stripeAmount,
                Currency = "eur", // Make sure this matches your desired currency
                Description = $"Depósito de {amount} EUR para el usuario {userId}",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true, // Highly recommended for Payment Element
                },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }, // Store the userId for later tracking
                    { "transactionType", "deposito" }
                }
            };

            var service = new PaymentIntentService();

            try
            {
                var paymentIntent = await service.CreateAsync(options);
                // Return the client_secret to the frontend
                return Result<string>.Success(paymentIntent.ClientSecret);
            }
            catch (StripeException ex)
            {
                Console.Error.WriteLine($"Error de Stripe al crear Payment Intent: {ex.Message}");
                return Result<string>.Failure($"Error al crear Payment Intent: {ex.Message}");
            }
        }




        public async Task<Result<Transaccion>> CrearTransaccion(Transaccion transaccion)
        {
            if (!IsValid(transaccion))
            {
                return Result<Transaccion>.Failure("Datos de transacción no válidos.");
            }

            var usuarioResult = await _usuariosService.GetUsuarioById(transaccion.UsuarioId);
            if (usuarioResult == null)
            {
                return Result<Transaccion>.Failure("El usuario no existe.");
            }
            var usuario = usuarioResult;

            transaccion.Fecha = DateTime.UtcNow;

            try
            {
                await _transaccionCollection.InsertOneAsync(transaccion);

                decimal cantidadAjuste = transaccion.TipoTransaccion.ToLower() == "retiro" || transaccion.TipoTransaccion.ToLower() == "apuesta"
                    ? -transaccion.Cantidad
                    : transaccion.Cantidad;

                var updateSaldoResult = await _usuariosService.UpdateUserBalance(transaccion.UsuarioId, cantidadAjuste);
                if (!updateSaldoResult.IsSuccess)
                {
                    return Result<Transaccion>.Failure("No se pudo actualizar el saldo del usuario. La transacción se ha registrado, pero el saldo del usuario no se ha modificado.");
                }

                return Result<Transaccion>.Success(transaccion);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al crear transacción: {ex.Message}");
                return Result<Transaccion>.Failure("Ocurrió un error al crear la transacción.");
            }
        }

        private async Task<Result<string>> ProcesarPago(string usuarioId, string tipoTransaccion, decimal cantidad, string metodoPago)
        {
            if (tipoTransaccion.ToLower() == "deposito")
            {
                // Crear un Cargo con Stripe
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(cantidad * 100), 
                    Currency = "eur", 
                    Description = $"Depósito de {cantidad} USD para el usuario {usuarioId}",
                    Source = metodoPago,   
                };
                var service = new ChargeService();
                try
                {
                    Charge charge = await service.CreateAsync(options);
                    if (charge.Status == "succeeded")
                    {
                        return Result<string>.Success("Pago procesado con éxito.");
                    }
                    else
                    {
                        return Result<string>.Failure($"Error al procesar el pago: {charge.FailureMessage}");
                    }
                }
                catch (StripeException e)
                {
                    Console.Error.WriteLine($"Error de Stripe: {e.Message}");
                    return Result<string>.Failure($"Error de Stripe: {e.Message}");
                }
            }
            else if (tipoTransaccion.ToLower() == "retiro")
            {
               
                var payoutOptions = new PayoutCreateOptions
                {
                    Amount = (long)(cantidad * 100),
                    Currency = "usd",
                    Destination = "bank_account[id]",  //Esto es un placeholder, Necesitas el ID de la cuenta bancaria del usuario
                };
                var payoutService = new PayoutService();
                try
                {
                    var payout = await payoutService.CreateAsync(payoutOptions);
                    if (payout.Status == "succeeded")
                    {
                        return Result<string>.Success("Retiro procesado con éxito");
                    }
                    else
                    {
                        return Result<string>.Failure($"Error al procesar el retiro: {payout.Status}");
                    }

                }
                catch (StripeException ex)
                {
                    Console.Error.WriteLine($"Error de Stripe: {ex.Message}");
                    return Result<string>.Failure($"Error de Stripe: {ex.Message}");
                }
            }
            else if (tipoTransaccion.ToLower() == "apuesta")
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(cantidad * 100), 
                    Currency = "eur", 
                    Description = $"Apuesta de {cantidad} € del usuario {usuarioId}",
                    Source = metodoPago,
                    Capture = false, 
                };
                var service = new ChargeService();
                try
                {
                    Charge charge = await service.CreateAsync(options);
                    if (charge.Status == "succeeded")
                    {
                        return Result<string>.Success("Apuesta procesada con éxito.");
                    }
                    else
                    {
                        return Result<string>.Failure($"Error al procesar la Apuesta: {charge.FailureMessage}");
                    }
                }
                catch (StripeException e)
                {
                    Console.Error.WriteLine($"Error de Stripe: {e.Message}");
                    return Result<string>.Failure($"Error de Stripe: {e.Message}");
                }
            }
            else if (tipoTransaccion.ToLower() == "ganancia")
            {
               
                var payoutOptions = new PayoutCreateOptions
                {
                    Amount = (long)(cantidad * 100),
                    Currency = "usd",
                    Destination = "bank_account[id]",  //Esto es un placeholder, Necesitas el ID de la cuenta bancaria del usuario
                };
                var payoutService = new PayoutService();
                try
                {
                    var payout = await payoutService.CreateAsync(payoutOptions);
                    if (payout.Status == "succeeded")
                    {
                        return Result<string>.Success("Ganancia procesada con éxito");
                    }
                    else
                    {
                        return Result<string>.Failure($"Error al procesar la Ganancia: {payout.Status}");
                    }
                }
                catch (StripeException ex)
                {
                    Console.Error.WriteLine($"Error de Stripe: {ex.Message}");
                    return Result<string>.Failure($"Error de Stripe: {ex.Message}");
                }
            }
            else
            {
                return Result<string>.Failure("Tipo de transacción no soportado.");
            }
        }

        public async Task<Transaccion?> GetTransaccionById(string id)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(id, out objectId))
            {
                return null;
            }

            return await _transaccionCollection.Find(t => t.Id == objectId.ToString()).FirstOrDefaultAsync();

        }

        public async Task<List<Transaccion>> GetTransaccionesByUsuarioId(string usuarioId, int page = 1, int pageSize = 10)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(usuarioId, out objectId))
            {
                return new List<Transaccion>();
            }

            var filter = Builders<Transaccion>.Filter.Eq(t => t.UsuarioId, objectId.ToString());
            return await _transaccionCollection.Find(filter)
                                                .Skip((page - 1) * pageSize)
                                                .Limit(pageSize)
                                                .ToListAsync();
        }

        private bool IsValid(Transaccion transaccion)
        {
            var context = new ValidationContext(transaccion);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(transaccion, context, results, true);
        }
    }
}
