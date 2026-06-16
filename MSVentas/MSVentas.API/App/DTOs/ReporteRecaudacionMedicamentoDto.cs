namespace MSVentas.App.DTOs
{
    public class ReporteRecaudacionMedicamentoDto
    {
        public int IdMedicamento { get; set; }
        public int CantidadVendida { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
