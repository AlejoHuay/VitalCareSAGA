namespace MSVentas.App.DTOs
{
    public class DetalleVentaInputDto
    {
        public int IdMedicamento { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
