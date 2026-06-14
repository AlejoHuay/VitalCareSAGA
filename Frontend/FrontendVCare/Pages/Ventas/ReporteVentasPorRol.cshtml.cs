using FrontendVCare.Adaptadores.Reportes;
using FrontendVCare.Dto.Reportes;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas
{
    public class ReporteVentasPorRolModel : BasePageModel
    {
        private readonly ReporteVentasAdapter reporteVentasAdapter;

        public ReporteVentasPorRolModel(ReporteVentasAdapter reporteVentasAdapter)
        {
            this.reporteVentasAdapter = reporteVentasAdapter;
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
                Reporte = await reporteVentasAdapter.ObtenerReporteVentasPorRolAsync(Desde, Hasta);
            }
            catch
            {
                MensajeError = "No se pudo cargar el reporte. Verifica que MSReportes este disponible.";
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDescargarPdfAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            ArchivoDescargaDto? archivo =
                await reporteVentasAdapter.DescargarReporteVentasPorRolPdfAsync(Desde, Hasta);

            if (archivo == null)
                return RedirectToPage(new { desde = Desde, hasta = Hasta });

            return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
        }

        public async Task<IActionResult> OnGetDescargarExcelAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null)
                return acceso;

            ArchivoDescargaDto? archivo =
                await reporteVentasAdapter.DescargarReporteVentasPorRolExcelAsync(Desde, Hasta);

            if (archivo == null)
                return RedirectToPage(new { desde = Desde, hasta = Hasta });

            return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
        }
    }
}
