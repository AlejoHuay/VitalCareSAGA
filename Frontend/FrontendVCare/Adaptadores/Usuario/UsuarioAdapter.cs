using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using Microsoft.AspNetCore.Http;

namespace FrontendVCare.Adaptadores
{
    public class UsuarioAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsuarioAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(OperacionApiDto Resultado, List<UsuarioDto> Usuarios)> ObtenerTodosConResultadoAsync(string? filtro = null)
        {
            string url = string.IsNullOrWhiteSpace(filtro)
                ? "api/usuarios/GetUsers"
                : $"api/usuarios/GetUsers?filtro={Uri.EscapeDataString(filtro)}";

            HttpResponseMessage response = await GetConTokenAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                string mensaje = await LeerMensajeAsync(response, "No se pudieron obtener los usuarios.");
                return (OperacionApiDto.Error(mensaje), new List<UsuarioDto>());
            }

            JsonElement? root = await LeerJsonAsync(response);
            if (root == null || !root.Value.TryGetProperty("data", out JsonElement dataElement) || dataElement.ValueKind != JsonValueKind.Array)
                return (OperacionApiDto.Error("La respuesta del servidor no contiene usuarios validos."), new List<UsuarioDto>());

            List<UsuarioDto> usuarios = dataElement.Deserialize<List<UsuarioDto>>(JsonOptions) ?? new List<UsuarioDto>();
            string mensajeExito = await LeerMensajeAsync(response, "Usuarios obtenidos correctamente.");
            return (OperacionApiDto.Ok(mensajeExito), usuarios);
        }

        public async Task<List<UsuarioDto>> ObtenerTodosAsync(string? filtro = null)
        {
            var (resultado, usuarios) = await ObtenerTodosConResultadoAsync(filtro);
            return resultado.Exito ? usuarios : new List<UsuarioDto>();
        }

        public async Task<(OperacionApiDto Resultado, UsuarioDto? Usuario)> ObtenerPorIdConResultadoAsync(int id)
        {
            HttpResponseMessage response = await GetConTokenAsync($"api/usuarios/getUserById?id={id}");
            if (!response.IsSuccessStatusCode)
            {
                string mensaje = await LeerMensajeAsync(response, "No se pudo obtener el usuario.");
                return (OperacionApiDto.Error(mensaje), null);
            }

            return await LeerUsuarioDetalleAsync(response, "Usuario obtenido correctamente.");
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        {
            var (resultado, usuario) = await ObtenerPorIdConResultadoAsync(id);
            return resultado.Exito ? usuario : null;
        }

        public async Task<(OperacionApiDto Resultado, UsuarioDto? Usuario)> ObtenerPorEmailConResultadoAsync(string email)
        {
            HttpResponseMessage response = await GetConTokenAsync($"api/usuarios/getUser?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode)
            {
                string mensaje = await LeerMensajeAsync(response, "No se pudo obtener el usuario.");
                return (OperacionApiDto.Error(mensaje), null);
            }

            return await LeerUsuarioDetalleAsync(response, "Usuario obtenido correctamente.");
        }

        public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email)
        {
            var (resultado, usuario) = await ObtenerPorEmailConResultadoAsync(email);
            return resultado.Exito ? usuario : null;
        }

        public async Task<(OperacionApiDto Resultado, UsuarioDto? Usuario)> ObtenerPorUserNameConResultadoAsync(string userName)
        {
            HttpResponseMessage response = await GetConTokenAsync($"api/usuarios/getUser?userName={Uri.EscapeDataString(userName)}");
            if (!response.IsSuccessStatusCode)
            {
                string mensaje = await LeerMensajeAsync(response, "No se pudo obtener el usuario.");
                return (OperacionApiDto.Error(mensaje), null);
            }

            return await LeerUsuarioDetalleAsync(response, "Usuario obtenido correctamente.");
        }

        public async Task<UsuarioDto?> ObtenerPorUserNameAsync(string userName)
        {
            var (resultado, usuario) = await ObtenerPorUserNameConResultadoAsync(userName);
            return resultado.Exito ? usuario : null;
        }

        public async Task<OperacionApiDto> CrearConResultadoAsync(UsuarioRegistroDto usuario)
        {
            HttpResponseMessage response = await PostConTokenAsync("api/usuarios/CrearUsuario", usuario);
            return await LeerResultadoAsync(response, "Usuario registrado correctamente.");
        }

        public async Task<(bool Success, string? Message)> CrearAsync(UsuarioRegistroDto usuario)
        {
            OperacionApiDto resultado = await CrearConResultadoAsync(usuario);
            return (resultado.Exito, resultado.Mensaje);
        }

        public async Task<OperacionApiDto> ActualizarConResultadoAsync(UsuarioActualizarDto usuario)
        {
            string url = "api/usuarios/actualizarUsuario";
            int? idUsuarioSesion = ObtenerIdUsuarioSesion();
            if (idUsuarioSesion.HasValue)
                url += $"?idUsuarioSesion={idUsuarioSesion.Value}";

            HttpResponseMessage response = await PutConTokenAsync(url, usuario);
            return await LeerResultadoAsync(response, "Usuario actualizado correctamente.");
        }

        public async Task<(bool Success, string? Message)> ActualizarAsync(UsuarioActualizarDto usuario)
        {
            OperacionApiDto resultado = await ActualizarConResultadoAsync(usuario);
            return (resultado.Exito, resultado.Mensaje);
        }

        public async Task<OperacionApiDto> EliminarAsync(int idUsuario)
        {
            int? idUsuarioSesion = ObtenerIdUsuarioSesion();
            if (!idUsuarioSesion.HasValue)
                return OperacionApiDto.Error("No se pudo identificar al usuario autenticado.");

            string url = $"api/usuarios/EliminarUsuario?idUsuario={idUsuario}&idUsuarioSesion={idUsuarioSesion.Value}";
            HttpResponseMessage response = await DeleteConTokenAsync(url);
            return await LeerResultadoAsync(response, "Usuario eliminado correctamente.");
        }

        public async Task<(bool Success, string? Message)> DarBajaLogicaAsync(int idUsuario)
        {
            OperacionApiDto resultado = await EliminarAsync(idUsuario);
            return (resultado.Exito, resultado.Mensaje);
        }

        private int? ObtenerIdUsuarioSesion()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32("IdUsuario");
        }

        private string? ObtenerToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Token");
        }

        private async Task<HttpResponseMessage> GetConTokenAsync(string url)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            AgregarBearer(request);
            return await _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> PostConTokenAsync(string url, object data)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(data)
            };
            AgregarBearer(request);
            return await _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> PutConTokenAsync(string url, object data)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(data)
            };
            AgregarBearer(request);
            return await _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> DeleteConTokenAsync(string url)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            AgregarBearer(request);
            return await _httpClient.SendAsync(request);
        }

        private void AgregarBearer(HttpRequestMessage request)
        {
            string? token = ObtenerToken();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static async Task<JsonElement?> LeerJsonAsync(HttpResponseMessage response)
        {
            string contenido = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(contenido))
                return null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(contenido);
                return document.RootElement.Clone();
            }
            catch
            {
                return null;
            }
        }

        private async Task<(OperacionApiDto Resultado, UsuarioDto? Usuario)> LeerUsuarioDetalleAsync(HttpResponseMessage response, string mensajeExito)
        {
            JsonElement? root = await LeerJsonAsync(response);
            if (root == null || !root.Value.TryGetProperty("data", out JsonElement dataElement) || dataElement.ValueKind != JsonValueKind.Object)
                return (OperacionApiDto.Error("La respuesta del servidor no contiene un usuario valido."), null);

            UsuarioDto? usuario = dataElement.Deserialize<UsuarioDto>(JsonOptions);
            if (usuario == null)
                return (OperacionApiDto.Error("No se pudo interpretar el usuario devuelto por el servidor."), null);

            string mensaje = await LeerMensajeAsync(response, mensajeExito);
            return (OperacionApiDto.Ok(mensaje), usuario);
        }

        private async Task<OperacionApiDto> LeerResultadoAsync(HttpResponseMessage response, string mensajeExito)
        {
            string mensaje = await LeerMensajeAsync(response, mensajeExito);
            return response.IsSuccessStatusCode
                ? OperacionApiDto.Ok(mensaje)
                : OperacionApiDto.Error(mensaje);
        }

        private async Task<string> LeerMensajeAsync(HttpResponseMessage response, string mensajePorDefecto)
        {
            JsonElement? root = await LeerJsonAsync(response);
            if (root == null)
                return mensajePorDefecto;

            if (root.Value.TryGetProperty("mensaje", out JsonElement mensajeElement))
            {
                string? mensaje = mensajeElement.GetString();
                if (!string.IsNullOrWhiteSpace(mensaje))
                    return mensaje;
            }

            return mensajePorDefecto;
        }
    }
}
