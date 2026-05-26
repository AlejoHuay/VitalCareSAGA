using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Helpers;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        [BindProperty]
        [RegularExpression(@"^$|^\d[A-Za-z0-9]{1,2}$", ErrorMessage = "El complemento del CI es opcional, pero si lo ingresas debe tener hasta 2 caracteres. Ej. 1A.")]
        public string? CiComplemento { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            var (resultado, usuario) = await _usuarioAdapter.ObtenerPorIdConResultadoAsync(id);
            if (!resultado.Exito || usuario == null || !EsBioquimico(usuario.Role))
                return RedirectToPage("Bioquimico", new { error = resultado.Mensaje });

            (string ciBase, string ciComplemento) = CiFormatoHelper.SepararCi(usuario.Ci);

            Input = new UsuarioActualizarDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno ?? string.Empty,
                Ci = ciBase,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = "Bioquimico",
                Activo = (byte)Math.Max(usuario.Activo, (sbyte)0),
                MustChangePassword = (byte)Math.Max(usuario.MustChangePassword, (sbyte)0)
            };
            CiComplemento = ciComplemento;

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

            string ciBase = Input.Ci;
            Input.Ci = CiFormatoHelper.ConstruirCi(Input.Ci, CiComplemento);

            Input.CiExtencion = Input.CiExtencion.Trim().ToUpperInvariant();
            Input.Email = Input.Email.Trim().ToLowerInvariant();

            OperacionApiDto resultado = await _usuarioAdapter.ActualizarConResultadoAsync(Input);
            if (!resultado.Exito)
            {
                Input.Ci = ciBase;
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
