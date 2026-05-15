using System.Text.Json.Serialization;

namespace MSUsuarios.App.DTOs
{
    public class UsuarioLoginRequestDto
    {
        [JsonPropertyName("email")]
        public string EmailOUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
