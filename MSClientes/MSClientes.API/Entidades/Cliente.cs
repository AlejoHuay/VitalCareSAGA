using MongoDB.Bson.Serialization.Attributes;

namespace MSClientes.API.Entidades
{
    [BsonIgnoreExtraElements]
    public class Cliente
    {
        public int IdCliente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public int? IdUsuario { get; set; }
        public short Estado { get; set; } = 1;
        public bool EsConsumidorFinal { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
