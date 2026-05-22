using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Usuario
{
    public class UsuarioModel : BasePageModel
    {
        private readonly UsuarioAdapter _usuarioAdapter;

        public List<UsuarioDto> Usuarios { get; set; } = new();

        public UsuarioModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public async Task<IActionResult> OnGetAsync(string? filtro, string? mensaje, string? error)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            CargarParametros(filtro, mensaje, error);
            await CargarUsuariosAsync(Estado.FiltroActual);
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarUsuarioLogicamenteAsync(int id, string? filtro)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            Estado.FiltroActual = (filtro ?? string.Empty).Trim();

            OperacionApiDto resultado = await _usuarioAdapter.EliminarAsync(id);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                await CargarUsuariosAsync(Estado.FiltroActual);
                return Page();
            }

            return RedirectToPage("Usuario", new
            {
                filtro = Estado.FiltroActual,
                mensaje = "Usuario dado de baja correctamente."
            });
        }

        private void CargarParametros(string? filtro, string? mensaje, string? error)
        {
            Estado.FiltroActual = (filtro ?? string.Empty).Trim();
            Estado.Mensaje = mensaje ?? string.Empty;
            Estado.MensajeError = error ?? string.Empty;
        }

        private async Task CargarUsuariosAsync(string filtro)
        {
            var (resultado, usuarios) = await _usuarioAdapter.ObtenerTodosConResultadoAsync(filtro);
            Usuarios = usuarios;

            if (!resultado.Exito && string.IsNullOrWhiteSpace(Estado.MensajeError))
                Estado.MensajeError = resultado.Mensaje;
        }
    }
}
