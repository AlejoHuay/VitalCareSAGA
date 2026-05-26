using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Helpers;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        [BindProperty]
        [RegularExpression(@"^$|^\d[A-Za-z]$", ErrorMessage = "El complemento del CI es opcional, pero si lo ingresas debe tener el formato 1A.")]
        public string? CiComplemento { get; set; }

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
                return Page();

            string ciBase = Registro.Ci;
            Registro.Ci = CiFormatoHelper.ConstruirCi(Registro.Ci, CiComplemento);

            Registro.CiExtencion = Registro.CiExtencion.Trim().ToUpperInvariant();

            Registro.Email = Registro.Email.Trim().ToLowerInvariant();

            OperacionApiDto resultado = await _usuarioAdapter.CrearConResultadoAsync(Registro);
            if (!resultado.Exito)
            {
                Registro.Ci = ciBase;
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Bioquimico", new { mensaje = resultado.Mensaje });
        }
    }
}
