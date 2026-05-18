using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FrontendVCare.Dto.Auth
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El carnet de identidad es obligatorio.")]
        public string Ci { get; set; } = string.Empty;

        [Required(ErrorMessage = "La extensión es obligatoria.")]
        public string CiExtencion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe contener solo dígitos numéricos.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; } = string.Empty;

        [JsonIgnore]
        [ValidateNever]
        public string UserName { get; set; } = string.Empty;

        [JsonIgnore]
        [ValidateNever]
        public string Password { get; set; } = string.Empty;
    }
}
