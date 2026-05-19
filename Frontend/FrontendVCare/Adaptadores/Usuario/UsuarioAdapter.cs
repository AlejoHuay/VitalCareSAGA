using System.Net.Http.Headers;
using System.Text.Json;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using FrontendVCare.Dto.Auth;

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

        private string? ObtenerToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Token");
        }

        private async Task<HttpResponseMessage> GetConTokenAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var token = ObtenerToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await _httpClient.SendAsync(request);
        }

        private async Task<HttpResponseMessage> PutConTokenAsync(string url, UsuarioActualizarDto data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(data)
            };
            var token = ObtenerToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await _httpClient.SendAsync(request);
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        {
            var response = await GetConTokenAsync($"api/usuarios/getUserById?id={id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return dataElement.Deserialize<UsuarioDto>(JsonOptions);
            }
            return null;
        }

        public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email)
        {
            var response = await GetConTokenAsync($"api/usuarios/getUser?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return dataElement.Deserialize<UsuarioDto>(JsonOptions);
            }
            return null;
        }

        public async Task<UsuarioDto?> ObtenerPorUserNameAsync(string userName)
        {
            var response = await GetConTokenAsync($"api/usuarios/getUser?userName={Uri.EscapeDataString(userName)}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return dataElement.Deserialize<UsuarioDto>(JsonOptions);
            }
            return null;
        }

        public async Task<List<UsuarioDto>> ObtenerTodosAsync(string? filtro = null)
        {
            string url = string.IsNullOrWhiteSpace(filtro)
                ? "api/usuarios/GetUsers"
                : $"api/usuarios/GetUsers?filtro={Uri.EscapeDataString(filtro)}";
            
            var response = await GetConTokenAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<UsuarioDto>();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return dataElement.Deserialize<List<UsuarioDto>>(JsonOptions) ?? new List<UsuarioDto>();
            }
            return new List<UsuarioDto>();
        }

        public async Task<(bool Success, string? Message)> ActualizarAsync(UsuarioActualizarDto usuario)
        {
            int? idUsuarioSesion = _httpContextAccessor.HttpContext?.Session.GetInt32("IdUsuario");
            string url = $"api/usuarios/actualizarUsuario";
            if (idUsuarioSesion.HasValue)
                url += $"?idUsuarioSesion={idUsuarioSesion}";
            
            var response = await PutConTokenAsync(url, usuario);
            
            if (response.IsSuccessStatusCode)
                return (true, "Usuario actualizado correctamente");

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensajeElement))
                    return (false, mensajeElement.GetString());
            }
            catch { }

            return (false, "Error al actualizar usuario");
        }

        private async Task<HttpResponseMessage> PostConTokenAsync(string url, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(data)
            };

            var token = ObtenerToken();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        public async Task<(bool Success, string? Message)> CrearAsync(UsuarioRegistroDto usuario)
        {
            var response = await PostConTokenAsync("api/usuarios/CrearUsuario", usuario);

            if (response.IsSuccessStatusCode)
                return (true, "Bioquímico registrado correctamente.");

            return (false, await LeerMensajeErrorAsync(response));
        }

        public async Task<(bool Success, string? Message)> DarBajaLogicaAsync(int idUsuario)
        {
            UsuarioDto? usuario = await ObtenerPorIdAsync(idUsuario);

            if (usuario == null)
                return (false, "Bioquímico no encontrado.");

            var update = new UsuarioActualizarDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty,
                Ci = usuario.Ci,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                Activo = 0,
                MustChangePassword = (byte)Math.Max(usuario.MustChangePassword, (sbyte)0)
            };

            return await ActualizarAsync(update);
        }

        private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("mensaje", out var mensaje))
                    return mensaje.GetString() ?? "Error desconocido.";
            }
            catch { }

            return "Error al procesar la solicitud.";
        }
    }
}

