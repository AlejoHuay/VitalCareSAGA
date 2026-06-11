using Microsoft.AspNetCore.Mvc;
using MSReportes.API.CasosDeUso.PuertosEntrada;

namespace MSReportes.API.AdaptadoresDeInterfaz.Controladores
{
    [ApiController]
    [Route("api/reportes/ventas")]
    public class ReporteVentasController : ControllerBase
    {
        private readonly IReporteVentasInputPort _reporteVentasInputPort;

        public ReporteVentasController(IReporteVentasInputPort reporteVentasInputPort)
        {
            _reporteVentasInputPort = reporteVentasInputPort;
        }

        [HttpGet("por-rol")]
        public async Task<IActionResult> ObtenerVentasPorRol()
        {
            var resultado = await _reporteVentasInputPort.ObtenerVentasPorRolAsync();

            return Ok(new
            {
                mensaje = "Reporte de ventas por rol generado correctamente.",
                fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                data = resultado
            });
        }

        [HttpGet("por-rol/pdf")]
        public async Task<IActionResult> DescargarPdf()
        {
            var archivo = await _reporteVentasInputPort.GenerarPdfVentasPorRolAsync();

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }

        [HttpGet("por-rol/excel")]
        public async Task<IActionResult> DescargarExcel()
        {
            var archivo = await _reporteVentasInputPort.GenerarExcelVentasPorRolAsync();

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo
            );
        }
    }
}