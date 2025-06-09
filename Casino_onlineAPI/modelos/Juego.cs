using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Casino_onlineAPI.modelos
{
    public class Juego
    {
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")] // Consider using "_id" as the standard MongoDB identifier
        public string? Id { get; set; }

        [BsonElement("nombreJuego")]
        [BsonRepresentation(BsonType.String)]
        public string NombreJuego { get; set; }

        [BsonElement("reglas")]
        [BsonRepresentation(BsonType.String)]
        public string Reglas { get; set; }

        [BsonElement("descripcion")]
        [BsonRepresentation(BsonType.String)]
        public string Descripcion { get; set; }
    }
}

