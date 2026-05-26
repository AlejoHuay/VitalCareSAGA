using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Helpers;
using FrontendVCare.Servicios;

namespace FrontendVCare.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly AuthClient _authClient;

        public LogoutModel(AuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await CerrarSesionAsync();
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CerrarSesionAsync();
            return RedirectToPage("/Index");
        }

        private async Task CerrarSesionAsync()
        {
            string? token = JwtSessionHelper.ObtenerToken(HttpContext);

            if (!string.IsNullOrWhiteSpace(token))
                await _authClient.LogoutAsync(token);

            JwtSessionHelper.EliminarToken(HttpContext);
        }
    }
}
