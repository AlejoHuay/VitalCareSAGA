namespace MSVentas.App.DTOs.Eventos
{
    public class StockFallidoEvent
    {
        public int IdVenta { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}