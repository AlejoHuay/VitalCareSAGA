using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Proveedor
{
    public class ProveedorModel : PageModel
    {
        private readonly ProveedorApiAdapter proveedorApiAdapter;

        public List<ProveedorDto> Proveedores { get; set; } = new();
        public string FiltroActual { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string MensajeError { get; set; } = string.Empty;

        public ProveedorModel(ProveedorApiAdapter proveedorApiAdapter)
        {
            this.proveedorApiAdapter = proveedorApiAdapter;
        }

        public async Task OnGetAsync(string? filtro, string? mensaje, string? error)
        {
            FiltroActual = filtro?.Trim() ?? string.Empty;
            Mensaje = mensaje ?? string.Empty;
            MensajeError = error ?? string.Empty;

            try
            {
                Proveedores = await proveedorApiAdapter.ObtenerTodosAsync(FiltroActual);
            }
            catch (HttpRequestException)
            {
                MensajeError = "No se pudo cargar proveedores. Verifica que MSProveedor este ejecutandose y que Neon responda.";
                Proveedores = new List<ProveedorDto>();
            }
        }

        public async Task<IActionResult> OnPostEliminarProveedorAsync(int id)
        {
            OperacionApiDto resultado = await proveedorApiAdapter.EliminarAsync(id);

            if (!resultado.Exito)
                return RedirectToPage("Proveedor", new { error = resultado.Mensaje });

            return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
        }
    }
}
