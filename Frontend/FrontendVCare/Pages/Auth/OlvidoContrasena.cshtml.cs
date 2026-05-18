using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Servicios;

namespace FrontendVCare.Pages.Auth
{
    public class OlvidoContrasenaModel : PageModel
    {
        private readonly AuthClient _authClient;

        public OlvidoContrasenaModel(AuthClient authClient)
        {
            _authClient = authClient;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
        public bool EsError { get; set; }
        public bool MostrarFormulario { get; set; } = true;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Email = Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                Mensaje = "El correo electronico es obligatorio.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                Mensaje = "Por favor, ingresa un correo electronico valido.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            SolicitarRecuperacionDto request = new SolicitarRecuperacionDto
            {
                Email = Email
            };

            OperacionApiDto resultado = await _authClient.SolicitarRecuperacionAsync(request);
            Mensaje = resultado.Mensaje;
            EsError = !resultado.Exito;
            MostrarFormulario = !resultado.Exito;
            return Page();
        }
    }
}
