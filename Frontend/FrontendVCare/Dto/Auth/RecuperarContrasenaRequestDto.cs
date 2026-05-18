namespace FrontendVCare.Dto.Auth
{
    public class RecuperarContrasenaRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
