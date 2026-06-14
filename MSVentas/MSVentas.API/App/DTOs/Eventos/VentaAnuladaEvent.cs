namespace MSVentas.App.DTOs.Eventos
{
    public class VentaAnuladaEvent
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public List<DetalleVentaAnuladaEvent> Detalles { get; set; } = new();
    }

    public class DetalleVentaAnuladaEvent
    {
        public int IdMedicamento { get; set; }
        public int Cantidad { get; set; }
    }
}
