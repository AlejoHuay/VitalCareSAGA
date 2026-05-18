using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontendVCare.Dto.Auth
{
    public class UsuarioLoginRequestDto
    {
        [Required(ErrorMessage = "El email o nombre de usuario es obligatorio.")]
        [JsonPropertyName("email")]
        public string EmailOUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
