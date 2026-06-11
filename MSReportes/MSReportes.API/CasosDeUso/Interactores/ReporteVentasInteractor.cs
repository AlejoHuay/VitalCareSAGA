using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.CasosDeUso.Builders;
using MSReportes.API.CasosDeUso.PuertosEntrada;
using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Interactores
{
    public class ReporteVentasInteractor : IReporteVentasInputPort
    {
        private readonly IReporteVentasRepositorio _reporteVentasRepositorio;
        private readonly IReporteVentasPdfCreador _reporteVentasPdfCreador;
        private readonly IReporteVentasExcelCreador _reporteVentasExcelCreador;
        private readonly IReporteVentasPorRolBuilder _reporteVentasPorRolBuilder;

        public ReporteVentasInteractor(
            IReporteVentasRepositorio reporteVentasRepositorio,
            IReporteVentasPdfCreador reporteVentasPdfCreador,
            IReporteVentasExcelCreador reporteVentasExcelCreador,
            IReporteVentasPorRolBuilder reporteVentasPorRolBuilder)
        {
            _reporteVentasRepositorio = reporteVentasRepositorio;
            _reporteVentasPdfCreador = reporteVentasPdfCreador;
            _reporteVentasExcelCreador = reporteVentasExcelCreador;
            _reporteVentasPorRolBuilder = reporteVentasPorRolBuilder;
        }

        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync()
        {
            return await _reporteVentasRepositorio.ObtenerVentasPorRolAsync();
        }

        public async Task<ArchivoReporteDto> GenerarPdfVentasPorRolAsync()
        {
            var datos = await _reporteVentasRepositorio.ObtenerVentasPorRolAsync();

            var reporte = _reporteVentasPorRolBuilder
                .ConEncabezado("VITALCARE", "REPORTE DE VENTAS POR ROL")
                .ConUsuarioGenerador("Usuario del sistema")
                .ConDetalle(datos)
                .ConResumen()
                .ConPiePagina("Reporte generado automáticamente por VitalCare.")
                .Build();

            return _reporteVentasPdfCreador.Crear(reporte);
        }

        public async Task<ArchivoReporteDto> GenerarExcelVentasPorRolAsync()
        {
            var datos = await _reporteVentasRepositorio.ObtenerVentasPorRolAsync();

            var reporte = _reporteVentasPorRolBuilder
                .ConEncabezado("VITALCARE", "REPORTE DE VENTAS POR ROL")
                .ConUsuarioGenerador("Usuario del sistema")
                .ConDetalle(datos)
                .ConResumen()
                .ConPiePagina("Reporte generado automáticamente por VitalCare.")
                .Build();

            return _reporteVentasExcelCreador.Crear(reporte);
        }
    }
}