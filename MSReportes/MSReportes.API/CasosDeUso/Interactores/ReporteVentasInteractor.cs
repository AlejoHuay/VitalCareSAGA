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
        private readonly IComprobanteVentaPdfCreador _comprobanteVentaPdfCreador;
        private readonly IReporteVentasPorRolBuilder _reporteVentasPorRolBuilder;

        public ReporteVentasInteractor(
            IReporteVentasRepositorio reporteVentasRepositorio,
            IReporteVentasPdfCreador reporteVentasPdfCreador,
            IReporteVentasExcelCreador reporteVentasExcelCreador,
            IComprobanteVentaPdfCreador comprobanteVentaPdfCreador,
            IReporteVentasPorRolBuilder reporteVentasPorRolBuilder)
        {
            _reporteVentasRepositorio = reporteVentasRepositorio;
            _reporteVentasPdfCreador = reporteVentasPdfCreador;
            _reporteVentasExcelCreador = reporteVentasExcelCreador;
            _comprobanteVentaPdfCreador = comprobanteVentaPdfCreador;
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

        public async Task<ArchivoReporteDto> GenerarComprobanteVentaPdfAsync(int idVenta)
        {
            if (idVenta <= 0)
                throw new ArgumentException("El id de venta no es valido.");

            ComprobanteVentaDto? comprobante =
                await _reporteVentasRepositorio.ObtenerComprobanteVentaAsync(idVenta);

            if (comprobante == null)
                throw new InvalidOperationException("La venta no existe o no se pudo cargar.");

            if (!string.Equals(comprobante.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Solo se puede generar comprobante de una venta activa.");

            if (!string.Equals(comprobante.EstadoSaga, "STOCK_CONFIRMADO", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El comprobante solo esta disponible cuando el stock fue confirmado.");

            if (comprobante.Detalles.Count == 0)
                throw new InvalidOperationException("La venta no tiene detalle de medicamentos.");

            return _comprobanteVentaPdfCreador.Crear(comprobante);
        }
    }
}
