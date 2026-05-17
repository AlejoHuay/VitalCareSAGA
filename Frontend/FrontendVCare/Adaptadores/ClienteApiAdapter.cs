using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Dto;

namespace FrontendVCare.Adaptadores
{
    public class ClienteApiAdapter
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ClienteApiAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<ClienteDto>> ObtenerTodosAsync(string filtro)
        {
            string url = string.IsNullOrWhiteSpace(filtro)
                ? "api/clientes"
                : $"api/clientes?filtro={WebUtility.UrlEncode(filtro)}";

            List<ClienteDto>? clientes = await httpClient.GetFromJsonAsync<List<ClienteDto>>(url, JsonOptions);
            return clientes ?? new List<ClienteDto>();
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/clientes/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ClienteDto>(JsonOptions);
        }

        public async Task<OperacionApiDto> CrearAsync(ClienteFormularioDto cliente)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/clientes", cliente);
            return await LeerResultadoAsync(response, "Cliente registrado correctamente.");
        }

        public async Task<OperacionApiDto> ActualizarAsync(int id, ClienteFormularioDto cliente)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/clientes/{id}", cliente);
            return await LeerResultadoAsync(response, "Cliente actualizado correctamente.");
        }

        public async Task<OperacionApiDto> EliminarAsync(int id, int idUsuario)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"api/clientes/{id}?idUsuario={idUsuario}");
            return await LeerResultadoAsync(response, "Cliente eliminado correctamente.");
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
    }
}
