using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteVentasRepositorio
    {
        Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync(DateTime? desde, DateTime? hasta);
        Task<IEnumerable<ReporteRecaudacionMedicamentoDto>> ObtenerRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta);
        Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(int idVenta);
    }
}
