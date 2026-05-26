using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Helpers;

namespace FrontendVCare.Pages
{
    public class IndexModel : PageModel
    {
        public string? Usuario { get; private set; }
        public string? Role { get; private set; }

        public void OnGet()
        {
            Usuario = JwtSessionHelper.ObtenerUserName(HttpContext);
            Role = JwtSessionHelper.ObtenerRole(HttpContext)?.Trim() ?? "Usuario";
        }
    }   
}
