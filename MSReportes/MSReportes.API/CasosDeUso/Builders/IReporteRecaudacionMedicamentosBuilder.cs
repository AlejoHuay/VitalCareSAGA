using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Builders
{
    public interface IReporteRecaudacionMedicamentosBuilder
    {
        IReporteRecaudacionMedicamentosBuilder ConEncabezado(string nombreEmpresa, string titulo);
        IReporteRecaudacionMedicamentosBuilder ConUsuarioGenerador(string usuarioGenerador);
        IReporteRecaudacionMedicamentosBuilder ConPeriodo(DateTime? desde, DateTime? hasta);
        IReporteRecaudacionMedicamentosBuilder ConDetalle(IEnumerable<ReporteRecaudacionMedicamentoDto> detalle);
        IReporteRecaudacionMedicamentosBuilder ConResumen();
        IReporteRecaudacionMedicamentosBuilder ConPiePagina(string piePagina);
        ReporteRecaudacionMedicamentos Build();
    }
}
