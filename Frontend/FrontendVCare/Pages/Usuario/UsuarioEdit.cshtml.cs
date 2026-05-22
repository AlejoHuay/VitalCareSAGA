using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Usuario
{
    public class UsuarioEditModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        [BindProperty]
        public UsuarioActualizarDto Input { get; set; } = new();

        public UsuarioEditModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public IActionResult OnGet()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            return Page();
        }

        public async Task<IActionResult> OnPostCargarUsuarioParaEdicionAsync(int id)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            var (resultado, usuario) = await _usuarioAdapter.ObtenerPorIdConResultadoAsync(id);
            if (!resultado.Exito || usuario == null)
                return RedirectToPage("Usuario", new { error = resultado.Mensaje });

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
                Role = usuario.Role,
                Activo = (byte)Math.Max(usuario.Activo, (sbyte)0),
                MustChangePassword = (byte)Math.Max(usuario.MustChangePassword, (sbyte)0)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostActualizarUsuarioAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            OperacionApiDto resultado = await _usuarioAdapter.ActualizarConResultadoAsync(Input);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Usuario", new { mensaje = "Perfil de usuario actualizado correctamente." });
        }
    }
}
