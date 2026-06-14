using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteVentasRepositorio
    {
        Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync();
        Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(int idVenta);
    }
}
