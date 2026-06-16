using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteRecaudacionMedicamentosPdfCreador
    {
        ArchivoReporteDto Crear(ReporteRecaudacionMedicamentos reporte);
    }
}
