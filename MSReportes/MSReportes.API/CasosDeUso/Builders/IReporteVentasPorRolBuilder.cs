using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Builders
{
    public interface IReporteVentasPorRolBuilder
    {
        IReporteVentasPorRolBuilder ConEncabezado(string nombreEmpresa, string titulo);

        IReporteVentasPorRolBuilder ConUsuarioGenerador(string usuarioGenerador);

        IReporteVentasPorRolBuilder ConPeriodo(DateTime? desde, DateTime? hasta);

        IReporteVentasPorRolBuilder ConDetalle(IEnumerable<ReporteVentasPorRolDto> detalle);

        IReporteVentasPorRolBuilder ConResumen();

        IReporteVentasPorRolBuilder ConPiePagina(string piePagina);

        ReporteVentasPorRol Build();
    }
}
