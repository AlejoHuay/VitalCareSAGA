using System.Net.Http.Headers;
using FrontendVCare.Dto.Reportes;

namespace FrontendVCare.Adaptadores.Reportes
{
    public class ReporteVentasAdapter
    {
        private readonly HttpClient httpClient;

        public ReporteVentasAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ArchivoDescargaDto?> DescargarComprobanteVentaPdfAsync(int idVenta)
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"api/reportes/ventas/{idVenta}/comprobante/pdf");

            if (!response.IsSuccessStatusCode)
                return null;

            byte[] contenido = await response.Content.ReadAsByteArrayAsync();

            return new ArchivoDescargaDto
            {
                NombreArchivo = ObtenerNombreArchivo(response.Content.Headers.ContentDisposition, idVenta),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf",
                Contenido = contenido
            };
        }

        private static string ObtenerNombreArchivo(ContentDispositionHeaderValue? contentDisposition, int idVenta)
        {
            string? nombreArchivo = contentDisposition?.FileNameStar
                ?? contentDisposition?.FileName?.Trim('"');

            return string.IsNullOrWhiteSpace(nombreArchivo)
                ? $"comprobante-venta-{idVenta}.pdf"
                : nombreArchivo;
        }
    }
}
