using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Bioquimico
{
    public class BioquimicoModel : BasePageModel
    {
        private const string RolBioquimico = "Bioquimico";
        private readonly UsuarioAdapter _usuarioAdapter;

        public BioquimicoModel(UsuarioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public List<UsuarioDto> Bioquimicos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? filtro, string? mensaje, string? error)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            CargarParametros(filtro, mensaje, error);
            await CargarBioquimicosAsync(Estado.FiltroActual);
            return Page();
        }

        public async Task<IActionResult> OnPostDarBajaAsync(int id, string? filtro)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            Estado.FiltroActual = (filtro ?? string.Empty).Trim();

            OperacionApiDto resultado = await _usuarioAdapter.EliminarAsync(id);
            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                await CargarBioquimicosAsync(Estado.FiltroActual);
                return Page();
            }

            return RedirectToPage("Bioquimico", new
            {
                filtro = Estado.FiltroActual,
                mensaje = "Bioquimico dado de baja correctamente."
            });
        }

        private void CargarParametros(string? filtro, string? mensaje, string? error)
        {
            Estado.FiltroActual = (filtro ?? string.Empty).Trim();
            Estado.Mensaje = mensaje ?? string.Empty;
            Estado.MensajeError = error ?? string.Empty;
        }

        private async Task CargarBioquimicosAsync(string filtro)
        {
            var (resultado, usuarios) = await _usuarioAdapter.ObtenerTodosConResultadoAsync(filtro);
            Bioquimicos = usuarios
                .Where(u => string.Equals(u.Role?.Trim(), RolBioquimico, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!resultado.Exito && string.IsNullOrWhiteSpace(Estado.MensajeError))
                Estado.MensajeError = resultado.Mensaje;
        }
    }
}
