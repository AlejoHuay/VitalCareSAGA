using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Dto;
using FrontendVCare.Helpers;
using FrontendVCare.Servicios;
using System.ComponentModel.DataAnnotations;

namespace FrontendVCare.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly AuthClient _authClient;

        public RegisterModel(AuthClient authClient)
        {
            _authClient = authClient;
        }

        [BindProperty]
        public UsuarioRegistroDto Registro { get; set; } = new();

        [BindProperty]
        [RegularExpression(@"^$|^\d[A-Za-z]$", ErrorMessage = "El complemento del CI debe tener el formato 1A.")]
        public string CiComplemento { get; set; } = string.Empty;

        public string MensajeError { get; set; } = string.Empty;
        public string MensajeOk { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Registro.UserName");
            ModelState.Remove("Registro.Password");

            if (!ModelState.IsValid)
                return Page();

            string ciBase = Registro.Ci;
            Registro.Ci = CiFormatoHelper.ConstruirCi(Registro.Ci, CiComplemento);
            Registro.UserName = CredencialesHelper.GenerarUserName(
                Registro.Nombres,
                Registro.ApellidoPaterno,
                Registro.Ci
            );

            OperacionApiDto resultado = await _authClient.RegistrarAsync(Registro);

            if (!resultado.Exito)
            {
                Registro.Ci = ciBase;
                MensajeError = resultado.Mensaje;
                return Page();
            }

            MensajeOk = "Usuario registrado correctamente. Revisa las credenciales generadas y tu correo electronico.";
            return Page();
        }
    }
}
