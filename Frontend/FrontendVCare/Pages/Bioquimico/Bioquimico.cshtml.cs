using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoModel : PageModel
    {
        private const string RolBioquimico = "Bioquimico";
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public List<UsuarioDto> Bioquimicos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Filtro { get; set; } = string.Empty;

        [TempData]
        public string Mensaje { get; set; } = string.Empty;

        [TempData]
        public string MensajeError { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            List<UsuarioDto> usuarios = await _usuarioAdapter.ObtenerTodosAsync(Filtro);

            Bioquimicos = usuarios
                .Where(u => string.Equals(u.Role?.Trim(), RolBioquimico, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDarBajaAsync(int id)
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            var resultado = await _usuarioAdapter.DarBajaLogicaAsync(id);

            if (!resultado.Success)
                MensajeError = resultado.Message ?? "No se pudo dar de baja al bioquímico.";
            else
                Mensaje = "Bioquímico dado de baja correctamente.";

            return RedirectToPage(new { filtro = Filtro });
        }

        private bool EsAdmin()
        {
            string role = HttpContext.Session.GetString("Role") ?? string.Empty;
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}