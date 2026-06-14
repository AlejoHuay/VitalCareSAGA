using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Ventas;

namespace FrontendVCare.Adaptadores.Ventas
{
    public class VentaAdapter
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public VentaAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<VentaDto>> ObtenerTodasAsync(string filtro = "")
        {
            string url = string.IsNullOrWhiteSpace(filtro)
                ? "api/ventas"
                : $"api/ventas?filtro={WebUtility.UrlEncode(filtro)}";

            HttpResponseMessage response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            RespuestaVentasDto? respuesta =
                await response.Content.ReadFromJsonAsync<RespuestaVentasDto>(JsonOptions);

            return respuesta?.Data ?? new List<VentaDto>();
        }

        public async Task<VentaDto?> ObtenerPorIdAsync(int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/ventas/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            RespuestaVentaDto? respuesta =
                await response.Content.ReadFromJsonAsync<RespuestaVentaDto>(JsonOptions);

            return respuesta?.Data;
        }

        public async Task<List<VentaDetalleDto>> ObtenerDetallesAsync(int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/ventas/{id}/detalles");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new List<VentaDetalleDto>();

            response.EnsureSuccessStatusCode();

            RespuestaDetallesVentaDto? respuesta =
                await response.Content.ReadFromJsonAsync<RespuestaDetallesVentaDto>(JsonOptions);

            return respuesta?.Data ?? new List<VentaDetalleDto>();
        }

        public async Task<OperacionApiDto> RegistrarAsync(VentaFormularioDto venta)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/ventas", venta);
            return await LeerResultadoAsync(response, "Venta registrada correctamente.");
        }

        public async Task<OperacionApiDto> AnularAsync(int id, int idUsuario)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/ventas/{id}?idUsuario={idUsuario}");
            return await LeerResultadoAsync(response, "Venta anulada correctamente.");
        }

        public async Task<List<ReporteVentasPorRolDto>> ObtenerReportePorRolAsync(DateTime? desde, DateTime? hasta)
        {
            List<string> parametros = new();
            if (desde.HasValue)
                parametros.Add($"desde={WebUtility.UrlEncode(desde.Value.ToString("yyyy-MM-dd"))}");
            if (hasta.HasValue)
                parametros.Add($"hasta={WebUtility.UrlEncode(hasta.Value.ToString("yyyy-MM-dd"))}");

            string query = parametros.Count == 0 ? string.Empty : $"?{string.Join("&", parametros)}";
            List<ReporteVentasPorRolDto>? reporte = await httpClient.GetFromJsonAsync<List<ReporteVentasPorRolDto>>(
                $"api/ventas/reporte-por-rol{query}",
                JsonOptions);

            return reporte ?? new List<ReporteVentasPorRolDto>();
        }

        private static async Task<OperacionApiDto> LeerResultadoAsync(HttpResponseMessage response, string mensajeExito)
        {
            MensajeApiDto? respuesta = await LeerMensajeAsync(response);
            string mensaje = string.IsNullOrWhiteSpace(respuesta?.Mensaje)
                ? mensajeExito
                : respuesta.Mensaje;

            return response.IsSuccessStatusCode
                ? OperacionApiDto.Ok(mensaje)
                : OperacionApiDto.Error(mensaje);
        }

        private static async Task<MensajeApiDto?> LeerMensajeAsync(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<MensajeApiDto>(JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private class RespuestaVentasDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public List<VentaDto> Data { get; set; } = new();
        }

        private class RespuestaVentaDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public VentaDto? Data { get; set; }
        }

        private class RespuestaDetallesVentaDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public List<VentaDetalleDto> Data { get; set; } = new();
        }
    }
}
