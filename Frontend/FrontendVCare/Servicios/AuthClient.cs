using System.Net.Http.Headers;
using System.Text.Json;
using FrontendVCare.Adaptadores.Auth;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
namespace FrontendVCare.Servicios
{
    public class AuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly LoginResponseAdapter _loginResponseAdapter;
        private readonly MensajeApiAdapter _mensajeApiAdapter;

        public AuthClient(
            HttpClient httpClient,
            LoginResponseAdapter loginResponseAdapter,
            MensajeApiAdapter mensajeApiAdapter)
        {
            _httpClient = httpClient;
            _loginResponseAdapter = loginResponseAdapter;
            _mensajeApiAdapter = mensajeApiAdapter;
        }

        public async Task<(OperacionApiDto Resultado, UsuarioLoginResponseDto? Respuesta)> LoginAsync(UsuarioLoginRequestDto request)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode)
            {
                string mensajeError = await LeerMensajeAsync(response, "Credenciales incorrectas.");
                return (OperacionApiDto.Error(mensajeError), null);
            }

            JsonElement? json = await LeerJsonAsync(response);
            if (json == null)
                return (OperacionApiDto.Error("No se pudo leer la respuesta del servidor."), null);

            UsuarioLoginResponseDto respuesta = _loginResponseAdapter.Adapt(json.Value);
            if (string.IsNullOrWhiteSpace(respuesta.Token))
                return (OperacionApiDto.Error("El servidor no devolvió un token válido."), null);

            return (OperacionApiDto.Ok("Inicio de sesión correcto."), respuesta);
        }

        public async Task<OperacionApiDto> LogoutAsync(string token)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return await LeerResultadoAsync(response, "Sesión cerrada correctamente.");
        }

        public async Task<OperacionApiDto> CambiarContrasenaAsync(string token, string passwordActual, string nuevaPassword, string confirmarPassword)
        {
            Dictionary<string, string> datos = new Dictionary<string, string>
            {
                ["passwordActual"] = passwordActual,
                ["nuevaPassword"] = nuevaPassword,
                ["confirmarPassword"] = confirmarPassword
            };

            using FormUrlEncodedContent content = new FormUrlEncodedContent(datos);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/auth/cambiar-contrasena")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return await LeerResultadoAsync(response, "Contraseña actualizada correctamente.");
        }

        public async Task<OperacionApiDto> ActivarCuentaAsync(ActivarCuentaRequestDto request)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/activar-cuenta", request);
            return await LeerResultadoAsync(response, "Cuenta activada correctamente. Ahora puedes iniciar sesión.");
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
            try
            {
                string contenido = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(contenido))
                    return mensajePorDefecto;

                using JsonDocument document = JsonDocument.Parse(contenido);
                JsonElement root = document.RootElement;

                // Intenta leer "mensaje"
                if (root.TryGetProperty("mensaje", out JsonElement mensajeElement))
                {
                    string? mensaje = mensajeElement.GetString();
                    if (!string.IsNullOrWhiteSpace(mensaje))
                        return mensaje;
                }

                // Si no encuentra "mensaje", intenta usar el adaptador
                string adaptedMensaje = _mensajeApiAdapter.Adapt(root).Mensaje;
                if (!string.IsNullOrWhiteSpace(adaptedMensaje))
                    return adaptedMensaje;

                return mensajePorDefecto;
            }
            catch (Exception ex)
            {
                // Si hay un error parseando, registra y devuelve un mensaje genérico
                Console.WriteLine($"Error al leer respuesta: {ex.Message}");
                return $"Error en la solicitud: {response.StatusCode}";
            }
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
    }
}
