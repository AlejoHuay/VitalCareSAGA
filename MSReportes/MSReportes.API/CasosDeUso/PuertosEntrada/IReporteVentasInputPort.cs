using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.PuertosEntrada
{
    public interface IReporteVentasInputPort
    {
        Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync();
        Task<ArchivoReporteDto> GenerarPdfVentasPorRolAsync();
        Task<ArchivoReporteDto> GenerarExcelVentasPorRolAsync();
    }
}