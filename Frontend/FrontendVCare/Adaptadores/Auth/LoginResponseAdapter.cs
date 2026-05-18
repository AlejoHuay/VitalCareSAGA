using System.Text.Json;
using FrontendVCare.Dto.Auth;

namespace FrontendVCare.Adaptadores.Auth
{
    public class LoginResponseAdapter : AdapterJSON<UsuarioLoginResponseDto>
    {
        public LoginResponseAdapter(HttpClient httpClient) : base(httpClient)
        {
        }

        public UsuarioLoginResponseDto Adapt(JsonElement origen)
        {
            return new UsuarioLoginResponseDto
            {
                IdUsuario = origen.TryGetProperty("idUsuario", out JsonElement idUsuarioElement)
                    && idUsuarioElement.TryGetInt32(out int idUsuario)
                        ? idUsuario
                        : 0,
                UserName = origen.TryGetProperty("userName", out JsonElement userNameElement)
                    ? userNameElement.GetString() ?? string.Empty
                    : string.Empty,
                Role = origen.TryGetProperty("role", out JsonElement roleElement)
                    ? roleElement.GetString() ?? string.Empty
                    : string.Empty,
                MustChangePassword = origen.TryGetProperty("mustChangePassword", out JsonElement mustChangePasswordElement)
                    && mustChangePasswordElement.ValueKind is JsonValueKind.True or JsonValueKind.False
                        ? mustChangePasswordElement.GetBoolean()
                        : false,
                Token = origen.TryGetProperty("token", out JsonElement tokenElement)
                    ? tokenElement.GetString() ?? string.Empty
                    : string.Empty,
                ExpiraEn = origen.TryGetProperty("expiraEn", out JsonElement expiraEnElement)
                    && expiraEnElement.TryGetInt32(out int expiraEn)
                        ? expiraEn
                        : 0
            };
        }

        public List<UsuarioLoginResponseDto> AdaptList(IEnumerable<JsonElement> origen)
        {
            return origen.Select(Adapt).ToList();
        }
    }
}
