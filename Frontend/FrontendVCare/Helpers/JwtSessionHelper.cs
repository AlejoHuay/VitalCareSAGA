using System.Text;
using System.Text.Json;
using FrontendVCare.Dto.Auth;

namespace FrontendVCare.Helpers
{
    public static class JwtSessionHelper
    {
        private const string ClaimIdUsuario = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string ClaimUserName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string ClaimRole = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public static void GuardarSesion(HttpContext context, UsuarioLoginResponseDto respuesta)
        {
            context.Session.SetString("Token", respuesta.Token);

            if (respuesta.IdUsuario > 0)
                context.Session.SetInt32("IdUsuario", respuesta.IdUsuario);

            if (!string.IsNullOrWhiteSpace(respuesta.UserName))
                context.Session.SetString("UserName", respuesta.UserName);

            if (!string.IsNullOrWhiteSpace(respuesta.Role))
                context.Session.SetString("Role", respuesta.Role);

            context.Session.SetString("MustChangePassword", respuesta.MustChangePassword.ToString());

            CompletarSesionDesdeTokenSiFalta(context, respuesta.Token);
        }

        private static void CompletarSesionDesdeTokenSiFalta(HttpContext context, string token)
        {
            Dictionary<string, string> claims = LeerClaims(token);

            if (context.Session.GetInt32("IdUsuario") == null
                && claims.TryGetValue(ClaimIdUsuario, out string? idUsuarioValue)
                && int.TryParse(idUsuarioValue, out int idUsuario))
            {
                context.Session.SetInt32("IdUsuario", idUsuario);
            }

            if (string.IsNullOrWhiteSpace(context.Session.GetString("UserName"))
                && claims.TryGetValue(ClaimUserName, out string? userName)
                && !string.IsNullOrWhiteSpace(userName))
            {
                context.Session.SetString("UserName", userName);
            }

            if (string.IsNullOrWhiteSpace(context.Session.GetString("Role"))
                && claims.TryGetValue(ClaimRole, out string? role)
                && !string.IsNullOrWhiteSpace(role))
            {
                context.Session.SetString("Role", role);
            }
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
    }
}
