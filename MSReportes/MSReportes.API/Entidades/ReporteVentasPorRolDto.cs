namespace MSReportes.API.Entidades
{
    public class ReporteVentasPorRolDto
    {
        public string Rol { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
        public List<ReporteVentasPorUsuarioDto> Usuarios { get; set; } = new();
    }

    public class ReporteVentasPorUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
