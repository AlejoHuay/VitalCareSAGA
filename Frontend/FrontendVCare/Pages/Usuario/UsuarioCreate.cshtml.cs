using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Helpers;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FrontendVCare.Pages.Usuario
{
    public class UsuarioCreateModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        [BindProperty]
        public UsuarioRegistroDto Input { get; set; } = new();

        [BindProperty]
        [RegularExpression(@"^$|^\d[A-Za-z]$", ErrorMessage = "El complemento del CI debe tener el formato 1A.")]
        public string CiComplemento { get; set; } = string.Empty;

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
                return Page();

            string ciBase = Input.Ci;
            Input.Ci = CiFormatoHelper.ConstruirCi(Input.Ci, CiComplemento);

            OperacionApiDto resultado = await _usuarioAdapter.CrearConResultadoAsync(Input);
            if (!resultado.Exito)
            {
                Input.Ci = ciBase;
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
