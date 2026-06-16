using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Dto.Reportes;

namespace FrontendVCare.Adaptadores.Reportes
{
    public class ReporteVentasAdapter
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReporteVentasAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<ReporteVentasPorRolDto>> ObtenerReporteVentasPorRolAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"api/reportes/ventas/por-rol{ConstruirQueryFechas(desde, hasta)}");

            response.EnsureSuccessStatusCode();

            RespuestaReporteVentasPorRolDto? respuesta =
                await response.Content.ReadFromJsonAsync<RespuestaReporteVentasPorRolDto>(JsonOptions);

            return respuesta?.Data ?? new List<ReporteVentasPorRolDto>();
        }

        public Task<ArchivoDescargaDto?> DescargarReporteVentasPorRolPdfAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            return DescargarArchivoAsync(
                $"api/reportes/ventas/por-rol/pdf{ConstruirQueryFechas(desde, hasta)}",
                "reporte-ventas-por-rol.pdf");
        }

        public Task<ArchivoDescargaDto?> DescargarReporteVentasPorRolExcelAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            return DescargarArchivoAsync(
                $"api/reportes/ventas/por-rol/excel{ConstruirQueryFechas(desde, hasta)}",
                "reporte-ventas-por-rol.xlsx");
        }

        public async Task<List<ReporteRecaudacionMedicamentoDto>> ObtenerReporteRecaudacionMedicamentosAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"api/reportes/ventas/recaudacion-medicamentos{ConstruirQueryFechas(desde, hasta)}");

            response.EnsureSuccessStatusCode();

            RespuestaReporteRecaudacionMedicamentosDto? respuesta =
                await response.Content.ReadFromJsonAsync<RespuestaReporteRecaudacionMedicamentosDto>(JsonOptions);

            return respuesta?.Data ?? new List<ReporteRecaudacionMedicamentoDto>();
        }

        public Task<ArchivoDescargaDto?> DescargarReporteRecaudacionMedicamentosPdfAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            return DescargarArchivoAsync(
                $"api/reportes/ventas/recaudacion-medicamentos/pdf{ConstruirQueryFechas(desde, hasta)}",
                "reporte-recaudacion-medicamentos.pdf");
        }

        public Task<ArchivoDescargaDto?> DescargarReporteRecaudacionMedicamentosExcelAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            return DescargarArchivoAsync(
                $"api/reportes/ventas/recaudacion-medicamentos/excel{ConstruirQueryFechas(desde, hasta)}",
                "reporte-recaudacion-medicamentos.xlsx");
        }

        public async Task<ArchivoDescargaDto?> DescargarComprobanteVentaPdfAsync(int idVenta)
        {
            return await DescargarArchivoAsync(
                $"api/reportes/ventas/{idVenta}/comprobante/pdf",
                $"comprobante-venta-{idVenta}.pdf");
        }

        private async Task<ArchivoDescargaDto?> DescargarArchivoAsync(string url, string nombrePorDefecto)
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            byte[] contenido = await response.Content.ReadAsByteArrayAsync();

            return new ArchivoDescargaDto
            {
                NombreArchivo = ObtenerNombreArchivo(response.Content.Headers.ContentDisposition, nombrePorDefecto),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf",
                Contenido = contenido
            };
        }

        private static string ConstruirQueryFechas(DateTime? desde, DateTime? hasta)
        {
            List<string> parametros = new();

            if (desde.HasValue)
                parametros.Add($"desde={WebUtility.UrlEncode(desde.Value.ToString("yyyy-MM-dd"))}");

            if (hasta.HasValue)
                parametros.Add($"hasta={WebUtility.UrlEncode(hasta.Value.ToString("yyyy-MM-dd"))}");

            return parametros.Count == 0
                ? string.Empty
                : $"?{string.Join("&", parametros)}";
        }

        private static string ObtenerNombreArchivo(
            ContentDispositionHeaderValue? contentDisposition,
            string nombrePorDefecto)
        {
            string? nombreArchivo = contentDisposition?.FileNameStar
                ?? contentDisposition?.FileName?.Trim('"');

            return string.IsNullOrWhiteSpace(nombreArchivo)
                ? nombrePorDefecto
                : nombreArchivo;
        }

        private class RespuestaReporteVentasPorRolDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public List<ReporteVentasPorRolDto> Data { get; set; } = new();
        }

        private class RespuestaReporteRecaudacionMedicamentosDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public List<ReporteRecaudacionMedicamentoDto> Data { get; set; } = new();
        }
    }
}
