namespace FrontendVCare.Dto
{
    public class ClienteFormularioDto
    {
        public int IdCliente { get; set; }
        public int? IdUsuario { get; set; }
        public short Estado { get; set; } = 1;
        public bool EsConsumidorFinal { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
