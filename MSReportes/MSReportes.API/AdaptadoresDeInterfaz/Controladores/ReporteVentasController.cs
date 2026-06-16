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
        public async Task<IActionResult> ObtenerVentasPorRol(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var resultado = await _reporteVentasInputPort.ObtenerVentasPorRolAsync(desde, hasta);

                return Ok(new
                {
                    mensaje = "Reporte de ventas por rol generado correctamente.",
                    fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    data = resultado
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("por-rol/pdf")]
        public async Task<IActionResult> DescargarPdf(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var archivo = await _reporteVentasInputPort.GenerarPdfVentasPorRolAsync(desde, hasta);

                return File(
                    archivo.Contenido,
                    archivo.ContentType,
                    archivo.NombreArchivo
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("por-rol/excel")]
        public async Task<IActionResult> DescargarExcel(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var archivo = await _reporteVentasInputPort.GenerarExcelVentasPorRolAsync(desde, hasta);

                return File(
                    archivo.Contenido,
                    archivo.ContentType,
                    archivo.NombreArchivo
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("recaudacion-medicamentos")]
        public async Task<IActionResult> ObtenerRecaudacionPorMedicamento(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var resultado = await _reporteVentasInputPort.ObtenerRecaudacionPorMedicamentoAsync(desde, hasta);

                return Ok(new
                {
                    mensaje = "Reporte de recaudacion por medicamentos generado correctamente.",
                    fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    data = resultado
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("recaudacion-medicamentos/pdf")]
        public async Task<IActionResult> DescargarRecaudacionMedicamentosPdf(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var archivo = await _reporteVentasInputPort.GenerarPdfRecaudacionPorMedicamentoAsync(desde, hasta);

                return File(
                    archivo.Contenido,
                    archivo.ContentType,
                    archivo.NombreArchivo
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("recaudacion-medicamentos/excel")]
        public async Task<IActionResult> DescargarRecaudacionMedicamentosExcel(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                var archivo = await _reporteVentasInputPort.GenerarExcelRecaudacionPorMedicamentoAsync(desde, hasta);

                return File(
                    archivo.Contenido,
                    archivo.ContentType,
                    archivo.NombreArchivo
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{idVenta:int}/comprobante/pdf")]
        public async Task<IActionResult> DescargarComprobantePdf(int idVenta)
        {
            try
            {
                var archivo = await _reporteVentasInputPort.GenerarComprobanteVentaPdfAsync(idVenta);

                return File(
                    archivo.Contenido,
                    archivo.ContentType,
                    archivo.NombreArchivo
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
