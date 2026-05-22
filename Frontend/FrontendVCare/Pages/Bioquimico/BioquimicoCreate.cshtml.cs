using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoCreateModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoCreateModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        [BindProperty]
        public UsuarioRegistroDto Registro { get; set; } = new();

        public IActionResult OnGet()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            Registro.Role = "Bioquimico";

            ModelState.Remove("Registro.UserName");
            ModelState.Remove("Registro.Password");

            if (!ModelState.IsValid)
            {
                Estado.MensajeError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                    ?? "Verifica los datos del formulario.";
                return Page();
            }

            OperacionApiDto resultado = await _usuarioAdapter.CrearConResultadoAsync(Registro);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Bioquimico", new { mensaje = resultado.Mensaje });
        }
    }
}
