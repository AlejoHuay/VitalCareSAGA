using MSReportes.API.Entidades;

namespace MSReportes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IReporteRecaudacionMedicamentosExcelCreador
    {
        ArchivoReporteDto Crear(ReporteRecaudacionMedicamentos reporte);
    }
}
