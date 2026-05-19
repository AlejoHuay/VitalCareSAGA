using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoCreateModel : PageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoCreateModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        [BindProperty]
        public UsuarioRegistroDto Registro { get; set; } = new();

        [BindProperty]
        public string? CiBase { get; set; }

        [BindProperty]
        public string? CiComplemento { get; set; }

        public string MensajeError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            Registro.Role = "Bioquimico";
            Registro.Ci = ConstruirCi(CiBase, CiComplemento);

            ModelState.Remove("Registro.UserName");
            ModelState.Remove("Registro.Password");

            if (!ModelState.IsValid)
            {
                MensajeError = "Verifica los datos del formulario.";
                return Page();
            }

            var resultado = await _usuarioAdapter.CrearAsync(Registro);

            if (!resultado.Success)
            {
                MensajeError = resultado.Message ?? "No se pudo registrar el bioquímico.";
                return Page();
            }

            TempData["Mensaje"] = "Bioquímico registrado correctamente. Revisa el correo para activar la cuenta.";
            return RedirectToPage("Bioquimico");
        }

        private bool EsAdmin()
        {
            string role = HttpContext.Session.GetString("Role") ?? string.Empty;
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string ConstruirCi(string? ciBase, string? ciComplemento)
        {
            string baseLimpia = (ciBase ?? string.Empty).Trim();
            string complemento = (ciComplemento ?? string.Empty).Trim().ToUpperInvariant();

            return string.IsNullOrWhiteSpace(complemento)
                ? baseLimpia
                : $"{baseLimpia}-{complemento}";
        }
    }
}