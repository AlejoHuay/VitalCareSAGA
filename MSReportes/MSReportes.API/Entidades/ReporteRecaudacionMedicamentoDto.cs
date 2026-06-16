namespace MSReportes.API.Entidades
{
    public class ReporteRecaudacionMedicamentoDto
    {
        public int IdMedicamento { get; set; }
        public string Medicamento { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
