using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoEditModel : PageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoEditModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        [BindProperty]
        public UsuarioActualizarDto Input { get; set; } = new();

        [BindProperty]
        public string? CiBase { get; set; }

        [BindProperty]
        public string? CiComplemento { get; set; }

        public string MensajeError { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            UsuarioDto? usuario = await _usuarioAdapter.ObtenerPorIdAsync(id);

            if (usuario == null || !EsBioquimico(usuario.Role))
            {
                TempData["MensajeError"] = "Bioquímico no encontrado.";
                return RedirectToPage("Bioquimico");
            }

            CargarInput(usuario);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!EsAdmin())
                return RedirectToPage("/Auth/Login");

            Input.Role = "Bioquimico";
            Input.Ci = ConstruirCi(CiBase, CiComplemento);

            var resultado = await _usuarioAdapter.ActualizarAsync(Input);

            if (!resultado.Success)
            {
                MensajeError = resultado.Message ?? "No se pudo actualizar el bioquímico.";
                return Page();
            }

            TempData["Mensaje"] = "Bioquímico actualizado correctamente.";
            return RedirectToPage("Bioquimico");
        }

        private void CargarInput(UsuarioDto usuario)
        {
            SepararCi(usuario.Ci);

            Input = new UsuarioActualizarDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty,
                Ci = usuario.Ci,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = "Bioquimico",
                Activo = (byte)Math.Max(usuario.Activo, (sbyte)0),
                MustChangePassword = (byte)Math.Max(usuario.MustChangePassword, (sbyte)0)
            };
        }

        private void SepararCi(string? ci)
        {
            string valor = ci?.Trim() ?? string.Empty;
            int index = valor.IndexOf('-');

            CiBase = index >= 0 ? valor[..index] : valor;
            CiComplemento = index >= 0 ? valor[(index + 1)..] : string.Empty;
        }

        private bool EsAdmin()
        {
            string role = HttpContext.Session.GetString("Role") ?? string.Empty;
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsBioquimico(string? role)
        {
            return role?.Trim().Equals("Bioquimico", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static string ConstruirCi(string? ciBase, string? ciComplemento)
        {
            string baseLimpia = (ciBase ?? string.Empty).Trim();
            string complemento = (ciComplemento ?? string.Empty).Trim().ToUpperInvariant();

            return string.IsNullOrWhiteSpace(complemento)
                ? baseLimpia
                : $"{baseLimpia}-{complemento}";
        }
    }
}