using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Dto.Auth;
using FrontendVCare.Dto;
using FrontendVCare.Helpers;
using FrontendVCare.Servicios;

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

        public string MensajeError { get; set; } = string.Empty;
        public string MensajeOk { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Registro.UserName = CredencialesHelper.GenerarUserName(
                Registro.Nombres,
                Registro.ApellidoPaterno,
                ConstruirCiParaUserName(Registro.Ci, Registro.CiComplemento)
            );

            ModelState.Remove("Registro.UserName");
            ModelState.Remove("Registro.Password");

            if (!ModelState.IsValid)
            {
                MensajeError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                    ?? "Verifica los datos del formulario.";
                return Page();
            }

            OperacionApiDto resultado = await _authClient.RegistrarAsync(Registro);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            MensajeOk = "Usuario registrado correctamente. Revisa las credenciales generadas y tu correo electronico.";
            return Page();
        }

        private static string ConstruirCiParaUserName(string ci, string? ciComplemento)
        {
            string ciBase = ci?.Trim() ?? string.Empty;
            string complemento = ciComplemento?.Trim().ToUpperInvariant() ?? string.Empty;

            return string.IsNullOrWhiteSpace(complemento)
                ? ciBase
                : $"{ciBase}-{complemento}";
        }
    }
}
