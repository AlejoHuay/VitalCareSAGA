namespace MSUsuarios.Dominio.Modelos
{
    public class Token
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public string TokenHash { get; set; }
        public string TipoToken { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool Revocado { get; set; }
        public bool Usado { get; set; }

        public DateTime? FechaUso { get; set; }
        public DateTime? FechaRevocacion { get; set; }

        public Token()
        {
            TokenHash = string.Empty;
            TipoToken = string.Empty;
        }

        public Token(int id, int usuarioId, string tokenHash, string tipoToken,
            DateTime fechaCreacion, DateTime fechaExpiracion, bool revocado, bool usado, DateTime? fechaUso, DateTime? fechaRevocacion)
        {
            Id = id;
            UsuarioId = usuarioId;
            TokenHash = tokenHash;
            TipoToken = tipoToken;
            FechaCreacion = fechaCreacion;
            FechaExpiracion = fechaExpiracion;
            Revocado = revocado;
            Usado = usado;
            FechaUso = fechaUso;
            FechaRevocacion = fechaRevocacion;
        }
    }
}