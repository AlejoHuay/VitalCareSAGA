using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Reportes;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto.Reportes;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas;

public class VerVentaModel : BasePageModel
{
    private readonly VentaAdapter ventaAdapter;
    private readonly MedicamentoAdapter medicamentoAdapter;
    private readonly ReporteVentasAdapter reporteVentasAdapter;

    public VerVentaModel(
        VentaAdapter ventaAdapter,
        MedicamentoAdapter medicamentoAdapter,
        ReporteVentasAdapter reporteVentasAdapter)
    {
        this.ventaAdapter = ventaAdapter;
        this.medicamentoAdapter = medicamentoAdapter;
        this.reporteVentasAdapter = reporteVentasAdapter;
    }

    public VentaDto? Venta { get; set; }

    public string? MensajeError { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool DescargarComprobante { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        try
        {
            Venta = await ventaAdapter.ObtenerPorIdAsync(id);

            if (Venta == null)
                return RedirectToPage("/Ventas/Venta", new { error = "Venta no encontrada." });

            await CompletarNombresMedicamentosAsync();
            return Page();
        }
        catch
        {
            return RedirectToPage("/Ventas/Venta", new { error = "No se pudo cargar el detalle de la venta." });
        }
    }

    public async Task<IActionResult> OnGetDescargarComprobanteAsync(int id)
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        try
        {
            ArchivoDescargaDto? archivo =
                await reporteVentasAdapter.DescargarComprobanteVentaPdfAsync(id);

            if (archivo == null)
            {
                return RedirectToPage("/Ventas/Venta", new
                {
                    error = "No se pudo generar el comprobante de la venta."
                });
            }

            return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
        }
        catch
        {
            return RedirectToPage("/Ventas/Venta", new
            {
                error = "No se pudo descargar el comprobante de la venta."
            });
        }
    }

    private async Task CompletarNombresMedicamentosAsync()
    {
        if (Venta == null)
            return;

        foreach (VentaDetalleDto detalle in Venta.Detalles)
        {
            try
            {
                var medicamento = await medicamentoAdapter.GetByIdAsync(detalle.IdMedicamento);
                detalle.Medicamento = medicamento == null
                    ? $"Medicamento #{detalle.IdMedicamento}"
                    : $"{medicamento.Nombre} - {medicamento.Presentacion}";
            }
            catch
            {
                detalle.Medicamento = $"Medicamento #{detalle.IdMedicamento}";
            }
        }
    }
}
