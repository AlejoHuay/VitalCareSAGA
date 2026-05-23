using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Base
{
    public abstract class BasePageModel : PageModel
    {
        public EstadoPagina Estado { get; set; } = new();

        protected IActionResult? ValidarAccesoAdmin()
        {
            string? usuario = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirectToPage("/Auth/Login");

            string role = HttpContext.Session.GetString("Role")?.Trim() ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/Index");

            return null;
        }

        protected int? ObtenerIdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }
    }
}
