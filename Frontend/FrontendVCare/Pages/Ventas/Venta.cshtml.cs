using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas
{
    public class VentaModel : BasePageModel
    {
        private readonly VentaAdapter ventaAdapter;

        public VentaModel(VentaAdapter ventaAdapter)
        {
            this.ventaAdapter = ventaAdapter;
        }

        public List<VentaDto> Ventas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Filtro { get; set; } = string.Empty;

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(string? mensaje = null, string? error = null)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            Mensaje = mensaje;
            MensajeError = error;

            await CargarDatosAsync(Filtro);
            return Page();
        }

        public async Task<IActionResult> OnPostAnularAsync(int id)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            int? idUsuario = ObtenerIdUsuarioSesion();
            if (idUsuario == null || idUsuario.Value == 0)
                return RedirectToPage("/Ventas/Venta", new { error = "No se encontro el usuario. Inicia sesion nuevamente." });

            OperacionApiDto resultado = await ventaAdapter.AnularAsync(id, idUsuario.Value);
            return RedirectToPage("/Ventas/Venta", resultado.Exito
                ? new { mensaje = resultado.Mensaje }
                : new { error = resultado.Mensaje });
        }

        private async Task CargarDatosAsync(string filtro)
        {
            try
            {
                Ventas = await ventaAdapter.ObtenerTodasAsync(filtro);
            }
            catch
            {
                MensajeError ??= "No se pudo cargar ventas. Verifica que MSVentas este disponible.";
            }
        }
    }
}
