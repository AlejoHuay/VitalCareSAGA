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
        private readonly IReporteRecaudacionMedicamentosPdfCreador _reporteRecaudacionMedicamentosPdfCreador;
        private readonly IReporteRecaudacionMedicamentosExcelCreador _reporteRecaudacionMedicamentosExcelCreador;
        private readonly IComprobanteVentaPdfCreador _comprobanteVentaPdfCreador;
        private readonly IReporteVentasPorRolBuilder _reporteVentasPorRolBuilder;
        private readonly IReporteRecaudacionMedicamentosBuilder _reporteRecaudacionMedicamentosBuilder;

        public ReporteVentasInteractor(
            IReporteVentasRepositorio reporteVentasRepositorio,
            IReporteVentasPdfCreador reporteVentasPdfCreador,
            IReporteVentasExcelCreador reporteVentasExcelCreador,
            IComprobanteVentaPdfCreador comprobanteVentaPdfCreador,
            IReporteVentasPorRolBuilder reporteVentasPorRolBuilder,
            IReporteRecaudacionMedicamentosPdfCreador reporteRecaudacionMedicamentosPdfCreador,
            IReporteRecaudacionMedicamentosExcelCreador reporteRecaudacionMedicamentosExcelCreador,
            IReporteRecaudacionMedicamentosBuilder reporteRecaudacionMedicamentosBuilder)
        {
            _reporteVentasRepositorio = reporteVentasRepositorio;
            _reporteVentasPdfCreador = reporteVentasPdfCreador;
            _reporteVentasExcelCreador = reporteVentasExcelCreador;
            _comprobanteVentaPdfCreador = comprobanteVentaPdfCreador;
            _reporteVentasPorRolBuilder = reporteVentasPorRolBuilder;
            _reporteRecaudacionMedicamentosPdfCreador = reporteRecaudacionMedicamentosPdfCreador;
            _reporteRecaudacionMedicamentosExcelCreador = reporteRecaudacionMedicamentosExcelCreador;
            _reporteRecaudacionMedicamentosBuilder = reporteRecaudacionMedicamentosBuilder;
        }

        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            return await _reporteVentasRepositorio.ObtenerVentasPorRolAsync(desde, hasta);
        }

        public async Task<ArchivoReporteDto> GenerarPdfVentasPorRolAsync(DateTime? desde, DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            var datos = await _reporteVentasRepositorio.ObtenerVentasPorRolAsync(desde, hasta);

            var reporte = CrearReporteVentasPorRol(datos, desde, hasta);

            return _reporteVentasPdfCreador.Crear(reporte);
        }

        public async Task<ArchivoReporteDto> GenerarExcelVentasPorRolAsync(DateTime? desde, DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            var datos = await _reporteVentasRepositorio.ObtenerVentasPorRolAsync(desde, hasta);

            var reporte = CrearReporteVentasPorRol(datos, desde, hasta);

            return _reporteVentasExcelCreador.Crear(reporte);
        }

        public async Task<IEnumerable<ReporteRecaudacionMedicamentoDto>> ObtenerRecaudacionPorMedicamentoAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            return await _reporteVentasRepositorio.ObtenerRecaudacionPorMedicamentoAsync(desde, hasta);
        }

        public async Task<ArchivoReporteDto> GenerarPdfRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            var datos = await _reporteVentasRepositorio.ObtenerRecaudacionPorMedicamentoAsync(desde, hasta);

            var reporte = CrearReporteRecaudacionMedicamentos(datos, desde, hasta);

            return _reporteRecaudacionMedicamentosPdfCreador.Crear(reporte);
        }

        public async Task<ArchivoReporteDto> GenerarExcelRecaudacionPorMedicamentoAsync(DateTime? desde, DateTime? hasta)
        {
            ValidarRangoFechas(desde, hasta);
            var datos = await _reporteVentasRepositorio.ObtenerRecaudacionPorMedicamentoAsync(desde, hasta);

            var reporte = CrearReporteRecaudacionMedicamentos(datos, desde, hasta);

            return _reporteRecaudacionMedicamentosExcelCreador.Crear(reporte);
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

        private ReporteVentasPorRol CrearReporteVentasPorRol(
            IEnumerable<ReporteVentasPorRolDto> datos,
            DateTime? desde,
            DateTime? hasta)
        {
            return _reporteVentasPorRolBuilder
                .ConEncabezado("VITALCARE", "REPORTE DE VENTAS POR ROL")
                .ConUsuarioGenerador("Usuario del sistema")
                .ConPeriodo(desde, hasta)
                .ConDetalle(datos)
                .ConResumen()
                .ConPiePagina("Reporte generado automaticamente por VitalCare.")
                .Build();
        }

        private ReporteRecaudacionMedicamentos CrearReporteRecaudacionMedicamentos(
            IEnumerable<ReporteRecaudacionMedicamentoDto> datos,
            DateTime? desde,
            DateTime? hasta)
        {
            return _reporteRecaudacionMedicamentosBuilder
                .ConEncabezado("VITALCARE", "REPORTE DE RECAUDACION POR MEDICAMENTOS")
                .ConUsuarioGenerador("Usuario del sistema")
                .ConPeriodo(desde, hasta)
                .ConDetalle(datos)
                .ConResumen()
                .ConPiePagina("Reporte generado automaticamente por VitalCare.")
                .Build();
        }

        private static void ValidarRangoFechas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde no puede ser mayor que la fecha hasta.");
        }
    }
}
