using System.ComponentModel.DataAnnotations;

namespace FrontendVCare.Dto
{
    public class UsuarioActualizarDto
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public byte Activo { get; set; } = 1;
        public string? UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "Los nombres solo deben contener letras.")]
        public string? Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El primer apellido solo debe contener letras.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]*$", ErrorMessage = "El segundo apellido solo debe contener letras.")]
        public string? ApellidoMaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El carnet de identidad es obligatorio.")]
        [RegularExpression(@"^\d{5,8}$", ErrorMessage = "El CI debe tener entre 5 y 8 dígitos numéricos.")]
        public string Ci { get; set; } = string.Empty;

        [Required(ErrorMessage = "La extensión del CI es obligatoria.")]
        public string CiExtencion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos numéricos.")]
        public string Telefono { get; set; } = string.Empty;

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
        public byte MustChangePassword { get; set; } = 1;
    }
}
