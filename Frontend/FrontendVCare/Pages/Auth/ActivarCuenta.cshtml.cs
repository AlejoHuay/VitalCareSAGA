using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Dto;
using FrontendVCare.Servicios;

namespace FrontendVCare.Pages.Auth
{
    public class ActivarCuentaModel : PageModel
    {
        private readonly AuthClient _authClient;

        public ActivarCuentaModel(AuthClient authClient)
        {
            _authClient = authClient;
        }

        [BindProperty]
        public ActivarCuentaRequestDto ActivarRequest { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;
        public string MensajeExito { get; set; } = string.Empty;

        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                MensajeError = "Token de activación no proporcionado.";
                return Page();
            }

            ActivarRequest.Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (string.IsNullOrWhiteSpace(ActivarRequest.Token))
            {
                MensajeError = "Token de activación no válido.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(ActivarRequest.NuevaPassword))
            {
                MensajeError = "La nueva contraseña es obligatoria.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(ActivarRequest.ConfirmarPassword))
            {
                MensajeError = "Debe confirmar la contraseña.";
                return Page();
            }

            if (ActivarRequest.NuevaPassword != ActivarRequest.ConfirmarPassword)
            {
                MensajeError = "La contraseña y su confirmación no coinciden.";
                return Page();
            }

            OperacionApiDto resultado = await _authClient.ActivarCuentaAsync(ActivarRequest);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje ?? "Error al activar la cuenta.";
                return Page();
            }

            MensajeExito = resultado.Mensaje ?? "Cuenta activada correctamente. Ahora puedes iniciar sesión.";
            return Page();
        }
    }
}
