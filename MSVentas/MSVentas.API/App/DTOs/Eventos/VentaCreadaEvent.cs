namespace MSVentas.App.DTOs.Eventos
{
    public class VentaCreadaEvent
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public List<DetalleVentaCreadaEvent> Detalles { get; set; } = new();
    }

    public class DetalleVentaCreadaEvent
    {
        public int IdMedicamento { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}