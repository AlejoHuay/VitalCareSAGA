using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Dto;

namespace FrontendVCare.Adaptadores
{
    public class ProveedorApiAdapter
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProveedorApiAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<ProveedorDto>> ObtenerTodosAsync(string filtro)
        {
            List<ProveedorDto>? proveedores = await httpClient.GetFromJsonAsync<List<ProveedorDto>>("api/proveedor", JsonOptions);
            proveedores ??= new List<ProveedorDto>();

            if (string.IsNullOrWhiteSpace(filtro))
                return proveedores;

            string texto = filtro.Trim();
            return proveedores
                .Where(p =>
                    Contiene(p.Nombre, texto) ||
                    Contiene(p.Telefono, texto) ||
                    Contiene(p.CorreoElectronico, texto) ||
                    Contiene(p.Direccion, texto))
                .ToList();
        }

        public async Task<ProveedorDto?> ObtenerPorIdAsync(int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/proveedor/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProveedorDto>(JsonOptions);
        }

        public async Task<OperacionApiDto> CrearAsync(ProveedorFormularioDto proveedor)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/proveedor", proveedor);
            return await LeerResultadoAsync(response, "Proveedor registrado correctamente.");
        }

        public async Task<OperacionApiDto> ActualizarAsync(int id, ProveedorFormularioDto proveedor)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/proveedor/{id}", proveedor);
            return await LeerResultadoAsync(response, "Proveedor actualizado correctamente.");
        }

        public async Task<OperacionApiDto> EliminarAsync(int id, int idUsuario)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/proveedor/{id}?idUsuario={idUsuario}");
            return await LeerResultadoAsync(response, "Proveedor eliminado correctamente.");
        }

        private static bool Contiene(string? valor, string filtro)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                   valor.Contains(filtro, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<OperacionApiDto> LeerResultadoAsync(HttpResponseMessage response, string mensajeExito)
        {
            string mensaje = await LeerMensajeAsync(response) ?? mensajeExito;

            return response.IsSuccessStatusCode
                ? OperacionApiDto.Ok(mensaje)
                : OperacionApiDto.Error(mensaje);
        }

        private static async Task<string?> LeerMensajeAsync(HttpResponseMessage response)
        {
            try
            {
                string contenido = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(contenido))
                    return null;

                using JsonDocument doc = JsonDocument.Parse(contenido);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("mensaje", out JsonElement mensaje))
                    return mensaje.GetString();

                if (root.TryGetProperty("Mensaje", out mensaje))
                    return mensaje.GetString();

                if (root.TryGetProperty("error", out JsonElement error))
                    return error.GetString();

                if (root.TryGetProperty("Error", out error))
                    return error.GetString();
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}