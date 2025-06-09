using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Casino_onlineAPI.modelos
{
    public class Partida
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        [BsonElement("usuarioId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UsuarioId { get; set; } // Referencia al _id del Usuario

        [BsonElement("juegoId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? JuegoId { get; set; }         // Referencia al _id del Juego

        [BsonElement("cantidadApostada")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CantidadApostada { get; set; }

        [BsonElement("resultado")]
        [BsonRepresentation(BsonType.String)]
        public string? Resultado { get; set; }

        [BsonElement("fecha")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Fecha { get; set; }
    }
}
