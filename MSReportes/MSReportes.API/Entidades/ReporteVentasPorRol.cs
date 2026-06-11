namespace MSReportes.API.Entidades
{
    public class ReporteVentasPorRol
    {
        public string Titulo { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public string UsuarioGenerador { get; set; } = string.Empty;

        public List<ReporteVentasPorRolDto> Detalle { get; set; } = new();

        public ResumenReporteVentas Resumen { get; set; } = new();

        public string PiePagina { get; set; } = string.Empty;
    }
}