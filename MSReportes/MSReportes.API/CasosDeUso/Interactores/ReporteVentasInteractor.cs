using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.CasosDeUso.PuertosEntrada;
using MSReportes.API.Entidades;

namespace MSReportes.API.CasosDeUso.Interactores
{
    public class ReporteVentasInteractor : IReporteVentasInputPort
    {
        private readonly IReporteVentasRepositorio _repositorio;
        private readonly IReporteVentasPdfCreador _pdfCreador;
        private readonly IReporteVentasExcelCreador _excelCreador;

        public ReporteVentasInteractor(
            IReporteVentasRepositorio repositorio,
            IReporteVentasPdfCreador pdfCreador,
            IReporteVentasExcelCreador excelCreador)
        {
            _repositorio = repositorio;
            _pdfCreador = pdfCreador;
            _excelCreador = excelCreador;
        }

        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync()
        {
            return await _repositorio.ObtenerVentasPorRolAsync();
        }

        public async Task<ArchivoReporteDto> GenerarPdfVentasPorRolAsync()
        {
            var datos = await _repositorio.ObtenerVentasPorRolAsync();
            return _pdfCreador.Crear(datos);
        }

        public async Task<ArchivoReporteDto> GenerarExcelVentasPorRolAsync()
        {
            var datos = await _repositorio.ObtenerVentasPorRolAsync();
            return _excelCreador.Crear(datos);
        }
    }
}