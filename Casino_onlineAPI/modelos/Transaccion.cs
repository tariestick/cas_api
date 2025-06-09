using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Casino_onlineAPI.modelos
{
    public class Transaccion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        [BsonElement("usuarioId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UsuarioId { get; set; } // Referencia al _id del Usuario

        [BsonElement("tipoTransaccion")]
        [BsonRepresentation(BsonType.String)]
        public string TipoTransaccion { get; set; } // "depósito", "retiro", "apuesta", "ganancia"

        [BsonElement("cantidad")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Cantidad { get; set; }

        [BsonElement("fecha")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Fecha { get; set; }


        [BsonElement("metodoPago")]
        [BsonRepresentation(BsonType.String)]
        public string MetodoPago { get; set; }

        
    }
}
