using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
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

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public string NuevaPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
        public bool EsError { get; set; }
        public bool MostrarFormulario { get; set; }

        public async Task<IActionResult> OnGetAsync(string token)
        {
            Token = token?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Token))
            {
                Mensaje = "Token inválido.";
                EsError = true;
                MostrarFormulario = false;
                return Page();
            }

            OperacionApiDto resultado = await _authClient.ValidarActivacionAsync(Token);
            Mensaje = resultado.Mensaje;
            EsError = !resultado.Exito;
            MostrarFormulario = resultado.Exito;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Token = Token?.Trim() ?? string.Empty;
            NuevaPassword = NuevaPassword?.Trim() ?? string.Empty;
            ConfirmarPassword = ConfirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Token))
            {
                Mensaje = "Token inválido.";
                EsError = true;
                MostrarFormulario = false;
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NuevaPassword))
            {
                Mensaje = "La nueva contraseña es obligatoria.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            if (NuevaPassword != ConfirmarPassword)
            {
                Mensaje = "La contraseña y su confirmación no coinciden.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            ActivarCuentaRequestDto request = new ActivarCuentaRequestDto
            {
                Token = Token,
                NuevaPassword = NuevaPassword,
                ConfirmarPassword = ConfirmarPassword
            };

            OperacionApiDto resultado = await _authClient.ActivarCuentaAsync(request);
            Mensaje = resultado.Mensaje;
            EsError = !resultado.Exito;
            MostrarFormulario = !resultado.Exito;
            return Page();
        }
    }
}