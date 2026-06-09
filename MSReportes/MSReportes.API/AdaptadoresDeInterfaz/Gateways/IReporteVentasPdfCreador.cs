using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteVentasPdfCreador
    {
        ArchivoReporteDto Crear(IEnumerable<ReporteVentasPorRolDto> datos);
    }
}