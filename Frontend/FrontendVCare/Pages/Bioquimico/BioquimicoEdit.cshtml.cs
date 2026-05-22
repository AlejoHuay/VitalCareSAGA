using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoEditModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoEditModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        [BindProperty]
        public UsuarioActualizarDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            var (resultado, usuario) = await _usuarioAdapter.ObtenerPorIdConResultadoAsync(id);
            if (!resultado.Exito || usuario == null || !EsBioquimico(usuario.Role))
                return RedirectToPage("Bioquimico", new { error = resultado.Mensaje });

            Input = new UsuarioActualizarDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty,
                Ci = usuario.Ci,
                CiComplemento = usuario.CiComplemento ?? string.Empty,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = "Bioquimico",
                Activo = (byte)Math.Max(usuario.Activo, (sbyte)0),
                MustChangePassword = (byte)Math.Max(usuario.MustChangePassword, (sbyte)0)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            Input.Role = "Bioquimico";

            if (!ModelState.IsValid)
                return Page();

            OperacionApiDto resultado = await _usuarioAdapter.ActualizarConResultadoAsync(Input);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Bioquimico", new { mensaje = "Bioquimico actualizado correctamente." });
        }

        private static bool EsBioquimico(string? role)
        {
            return role?.Trim().Equals("Bioquimico", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
