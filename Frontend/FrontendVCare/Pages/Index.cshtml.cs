using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoArqSoft.Pages
{
    public class IndexModel : PageModel
    {
        public string? Usuario { get; private set; }
        public string? Role { get; private set; }

        public void OnGet()
        {
            Usuario = HttpContext.Session.GetString("UserName");
            Role = HttpContext.Session.GetString("Role")?.Trim() ?? "Usuario";
        }
    }
}
