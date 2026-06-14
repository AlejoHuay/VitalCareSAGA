namespace MSReportes.API.Entidades
{
    public class ComprobanteVentaDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Cajero { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoSaga { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<ComprobanteVentaDetalleDto> Detalles { get; set; } = new();
    }
}
