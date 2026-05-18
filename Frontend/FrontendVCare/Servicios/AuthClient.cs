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

        public async Task<OperacionApiDto> RegistrarAsync(UsuarioRegistroDto request)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/registrar", request);
            return await LeerResultadoAsync(response, "Usuario registrado correctamente.");
        }

        public async Task<OperacionApiDto> ValidarActivacionAsync(string token)
        {
            string url = $"api/auth/validar-activacion?token={Uri.EscapeDataString(token)}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return await LeerResultadoAsync(response, "Token válido.");
        }

        public async Task<OperacionApiDto> ActivarCuentaAsync(ActivarCuentaRequestDto request)
        {
            Dictionary<string, string> datos = new Dictionary<string, string>
            {
                ["token"] = request.Token,
                ["nuevaPassword"] = request.NuevaPassword,
                ["confirmarPassword"] = request.ConfirmarPassword
            };

            using FormUrlEncodedContent content = new FormUrlEncodedContent(datos);
            HttpResponseMessage response = await _httpClient.PostAsync("api/auth/activar-cuenta", content);
            return await LeerResultadoAsync(response, "Cuenta activada correctamente.");
        }

        public async Task<OperacionApiDto> LogoutAsync(string token)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return await LeerResultadoAsync(response, "Sesión cerrada correctamente.");
        }

        public async Task<OperacionApiDto> SolicitarRecuperacionAsync(SolicitarRecuperacionDto request)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/solicitar-recuperacion-contrasena", request);
            return await LeerResultadoAsync(response, "Se ha enviado un enlace a tu correo.");
        }

        public async Task<OperacionApiDto> ValidarRecuperacionAsync(string token)
        {
            string url = $"api/auth/validar-recuperacion-contrasena?token={Uri.EscapeDataString(token)}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return await LeerResultadoAsync(response, "Token valido.");
        }

        public async Task<OperacionApiDto> ConfirmarRecuperacionAsync(RecuperarContrasenaRequestDto request)
        {
            Dictionary<string, string> datos = new Dictionary<string, string>
            {
                ["token"] = request.Token,
                ["nuevaPassword"] = request.NuevaPassword,
                ["confirmarPassword"] = request.ConfirmarPassword
            };

            using FormUrlEncodedContent content = new FormUrlEncodedContent(datos);
            HttpResponseMessage response = await _httpClient.PostAsync("api/auth/confirmar-recuperacion-contrasena", content);
            return await LeerResultadoAsync(response, "Contraseña actualizada correctamente.");
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
            JsonElement? json = await LeerJsonAsync(response);
            if (json == null)
                return mensajePorDefecto;

            string mensaje = _mensajeApiAdapter.Adapt(json.Value).Mensaje;
            return string.IsNullOrWhiteSpace(mensaje) ? mensajePorDefecto : mensaje;
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
