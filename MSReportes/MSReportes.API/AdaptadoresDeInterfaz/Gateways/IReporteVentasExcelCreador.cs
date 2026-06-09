using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteVentasExcelCreador
    {
        ArchivoReporteDto Crear(IEnumerable<ReporteVentasPorRolDto> datos);
    }
}