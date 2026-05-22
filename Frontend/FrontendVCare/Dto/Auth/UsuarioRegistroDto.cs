using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FrontendVCare.Dto.Auth
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "El campo Nombres es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Nombres no puede exceder 100 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\\s]+$", ErrorMessage = "El campo Nombres solo puede contener letras y espacios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Apellido Paterno no puede exceder 100 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\\s]+$", ErrorMessage = "El campo Apellido Paterno solo puede contener letras y espacios.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El campo Apellido Materno no puede exceder 100 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\\s]*$", ErrorMessage = "El campo Apellido Materno solo puede contener letras y espacios.")]
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El numero de carnet es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El CI debe tener 8 digitos numericos.")]
        public string Ci { get; set; } = string.Empty;

        [Required(ErrorMessage = "La extension del CI es obligatoria.")]
        public string CiExtencion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El telefono debe tener exactamente 8 digitos.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres.")]
        [EmailAddress(ErrorMessage = "El formato del email no es valido.")]
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
