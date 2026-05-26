using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Base
{
    public abstract class BasePageModel : PageModel
    {
        public EstadoPagina Estado { get; set; } = new();

        /// <summary>
        /// Valida que el usuario tenga uno de los roles permitidos
        /// </summary>
        /// <param name="rolesPermitidos">Roles permitidos para acceder a la página</param>
        /// <returns>Redirige a Login si no autenticado, a Index si rol no permitido, null si permitido</returns>
        protected IActionResult? ValidarAcceso(params string[] rolesPermitidos)
        {
            string? usuario = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirectToPage("/Auth/Login");

            string role = HttpContext.Session.GetString("Role")?.Trim() ?? string.Empty;
            
            bool tieneAcceso = rolesPermitidos.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
            if (!tieneAcceso)
                return RedirectToPage("/Index");

            return null;
        }

        /// <summary>
        /// Valida que el usuario tenga rol de Admin
        /// </summary>
        protected IActionResult? ValidarAccesoAdmin()
        {
            return ValidarAcceso("Admin");
        }

        protected int? ObtenerIdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }
    }
}
