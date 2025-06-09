using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Casino_onlineAPI.modelos
{
    public class Usuario
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        [BsonElement("nombreUsuario"),BsonRepresentation(BsonType.String)]
        public string? NombreUsuario { get; set; }

        [BsonElement("Contraseña"), BsonRepresentation(BsonType.String)]

        public string? Contraseña { get; set; }


        [BsonElement("correoElectronico"), BsonRepresentation(BsonType.String)]



        public string? CorreoElectronico { get; set; }
        [BsonElement("saldo"), BsonRepresentation(BsonType.Decimal128)]

        public decimal Saldo { get; set; }


        [BsonElement("historialJuegos"), BsonRepresentation(BsonType.String)]

        public List<string>? HistorialJuegos { get; set; } // Almacenará ObjectIds como strings (? maybe nose)

        [BsonElement("fechaRegistro"), BsonRepresentation(BsonType.DateTime)]

        public DateTime FechaRegistro { get; set; }
    }
}
