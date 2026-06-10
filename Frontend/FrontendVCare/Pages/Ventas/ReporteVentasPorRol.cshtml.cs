using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas
{
    public class ReporteVentasPorRolModel : BasePageModel
    {
        private readonly VentaAdapter ventaAdapter;

        public ReporteVentasPorRolModel(VentaAdapter ventaAdapter)
        {
            this.ventaAdapter = ventaAdapter;
        }

        public List<ReporteVentasPorRolDto> Reporte { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Hasta { get; set; }

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            try
            {
                Reporte = await ventaAdapter.ObtenerReportePorRolAsync(Desde, Hasta);
            }
            catch
            {
                MensajeError = "No se pudo cargar el reporte. Verifica que MSVentas este disponible.";
            }

            return Page();
        }
    }
}
