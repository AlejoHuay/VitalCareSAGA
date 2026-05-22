using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Usuario
{
    public class UsuarioCreateModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        [BindProperty]
        public UsuarioRegistroDto Input { get; set; } = new();

        public UsuarioCreateModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public IActionResult OnGet()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            return Page();
        }

        public async Task<IActionResult> OnPostCrearUsuarioAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            ModelState.Remove("Input.UserName");
            ModelState.Remove("Input.Password");

            if (!ModelState.IsValid)
            {
                Estado.MensajeError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                    ?? "Verifica los datos del formulario.";
                return Page();
            }

            OperacionApiDto resultado = await _usuarioAdapter.CrearConResultadoAsync(Input);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Usuario", new
            {
                mensaje = resultado.Mensaje
            });
        }
    }
}
