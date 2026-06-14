namespace FrontendVCare.Dto.Reportes
{
    public class ReporteVentasPorRolDto
    {
        public string Rol { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
