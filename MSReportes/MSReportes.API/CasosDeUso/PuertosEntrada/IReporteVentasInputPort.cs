using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.PuertosEntrada
{
    public interface IReporteVentasInputPort
    {
        Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync(DateTime? desde, DateTime? hasta);
        Task<ArchivoReporteDto> GenerarPdfVentasPorRolAsync(DateTime? desde, DateTime? hasta);
        Task<ArchivoReporteDto> GenerarExcelVentasPorRolAsync(DateTime? desde, DateTime? hasta);
        Task<IEnumerable<ReporteRecaudacionMedicamentoDto>> ObtenerRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta);
        Task<ArchivoReporteDto> GenerarPdfRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta);
        Task<ArchivoReporteDto> GenerarExcelRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta);
        Task<ArchivoReporteDto> GenerarComprobanteVentaPdfAsync(int idVenta);
    }
}
