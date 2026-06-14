using System.Text;
using System.Text.Json;
using FrontendVCare.Dto.Auth;

namespace FrontendVCare.Helpers
{
    public static class JwtSessionHelper
    {
        private const string TokenCookieName = "Authorization";
        private const string ClaimIdUsuario = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string ClaimUserName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string ClaimRole = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        /// <summary>
        /// Guarda el token en una cookie HTTP y extrae los datos del usuario
        /// </summary>
        public static void GuardarSesion(
            HttpContext context,
            UsuarioLoginResponseDto respuesta)
        {
            string token = NormalizarToken(respuesta.Token);

            if (token.Split('.').Length != 3)
            {
                throw new InvalidOperationException(
                    "El token recibido durante el login no tiene un formato JWT válido."
                );
            }

            context.Response.Cookies.Append(
                TokenCookieName,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                }
            );

            CompletarSesionDesdeTokenSiFalta(context, token);
        }

        /// <summary>
        /// Obtiene el token JWT desde la cookie
        /// </summary>
        public static string? ObtenerToken(HttpContext context)
        {
            context.Request.Cookies.TryGetValue(
                TokenCookieName,
                out string? token
            );

            string tokenLimpio = NormalizarToken(token);

            return string.IsNullOrWhiteSpace(tokenLimpio)
                ? null
                : tokenLimpio;
        }

        /// <summary>
        /// Elimina el token de la cookie
        /// </summary>
        public static void EliminarToken(HttpContext context)
        {
            context.Response.Cookies.Delete(TokenCookieName);
        }

        private static void CompletarSesionDesdeTokenSiFalta(HttpContext context, string token)
        {
            Dictionary<string, string> claims = LeerClaims(token);

            // Estos métodos ahora solo leen desde claims, no desde Session
            // Los datos se obtienen dinámicamente del token cada vez que se necesitan
        }

        /// <summary>
        /// Obtiene el ID del usuario desde el token
        /// </summary>
        public static int? ObtenerIdUsuario(HttpContext context)
        {
            string? token = ObtenerToken(context);
            if (string.IsNullOrWhiteSpace(token))
                return null;

            Dictionary<string, string> claims = LeerClaims(token);
            if (claims.TryGetValue(ClaimIdUsuario, out string? idValue) && int.TryParse(idValue, out int id))
                return id;

            return null;
        }

        /// <summary>
        /// Obtiene el nombre de usuario desde el token
        /// </summary>
        public static string? ObtenerUserName(HttpContext context)
        {
            string? token = ObtenerToken(context);
            if (string.IsNullOrWhiteSpace(token))
                return null;

            Dictionary<string, string> claims = LeerClaims(token);
            return claims.TryGetValue(ClaimUserName, out string? userName) ? userName : null;
        }

        /// <summary>
        /// Obtiene el rol desde el token
        /// </summary>
        public static string? ObtenerRole(HttpContext context)
        {
            string? token = ObtenerToken(context);
            if (string.IsNullOrWhiteSpace(token))
                return null;

            Dictionary<string, string> claims = LeerClaims(token);
            return claims.TryGetValue(ClaimRole, out string? role) ? role : null;
        }

        private static Dictionary<string, string> LeerClaims(string token)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();

            string[] partes = token.Split('.');
            if (partes.Length < 2)
                return claims;

            try
            {
                string payload = partes[1]
                    .Replace('-', '+')
                    .Replace('_', '/');

                int padding = 4 - (payload.Length % 4);
                if (padding is > 0 and < 4)
                    payload = payload.PadRight(payload.Length + padding, '=');

                byte[] bytes = Convert.FromBase64String(payload);
                using JsonDocument documento = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));

                foreach (JsonProperty propiedad in documento.RootElement.EnumerateObject())
                {
                    claims[propiedad.Name] = propiedad.Value.ToString();
                }
            }
            catch
            {
                return claims;
            }

            return claims;
        }

        private static string NormalizarToken(string? token)
        {
            string tokenLimpio = (token ?? string.Empty).Trim();

            tokenLimpio = tokenLimpio.Trim('"');

            if (tokenLimpio.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                tokenLimpio = tokenLimpio["Bearer ".Length..].Trim();
            }

            return tokenLimpio;
        }
    }
}
