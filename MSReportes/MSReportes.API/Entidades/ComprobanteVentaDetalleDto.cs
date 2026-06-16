namespace MSReportes.API.Entidades
{
    public class ComprobanteVentaDetalleDto
    {
        public int IdMedicamento { get; set; }
        public string Medicamento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
