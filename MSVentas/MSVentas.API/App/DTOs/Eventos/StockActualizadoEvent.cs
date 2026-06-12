namespace MSVentas.App.DTOs.Eventos
{
    public class StockActualizadoEvent
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public List<DetalleStockActualizadoEvent> Detalles { get; set; } = new();
    }

    public class DetalleStockActualizadoEvent
    {
        public int IdMedicamento { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}