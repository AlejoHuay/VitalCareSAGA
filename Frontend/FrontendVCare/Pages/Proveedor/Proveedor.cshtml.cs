using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrontendVCare.Pages.Proveedor
{
    public class ProveedorModel : BasePageModel
    {
        private readonly ProveedorApiAdapter proveedorApiAdapter;

        public List<ProveedorDto> Proveedores { get; set; } = new();
        public string FiltroActual { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        public ProveedorModel(ProveedorApiAdapter proveedorApiAdapter)
        {
            this.proveedorApiAdapter = proveedorApiAdapter;
        }

        public async Task<IActionResult> OnGetAsync(string? filtro, string? mensaje, string? error)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            FiltroActual = filtro?.Trim() ?? string.Empty;
            Mensaje = mensaje ?? string.Empty;
            
            if (!string.IsNullOrEmpty(error))
            {
                Estado.MensajeError = error;
            }

            try
            {
                Proveedores = await proveedorApiAdapter.ObtenerTodosAsync(FiltroActual);
            }
            catch (HttpRequestException)
            {
                Estado.MensajeError = "No se pudo cargar proveedores. Verifica que MSProveedor este ejecutandose y que Neon responda.";
                Proveedores = new List<ProveedorDto>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarProveedorAsync(int id)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            int idUsuarioActual = ObtenerIdUsuarioActual();

            if (idUsuarioActual <= 0)
            {
                return RedirectToPage("Proveedor", new { error = "Sesión inválida. Inicie sesión nuevamente." });
            }

            OperacionApiDto resultado = await proveedorApiAdapter.EliminarAsync(id, idUsuarioActual);

            if (!resultado.Exito)
                return RedirectToPage("Proveedor", new { error = resultado.Mensaje });

            return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
        }

        private int ObtenerIdUsuarioActual()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (int.TryParse(claimId, out int id)) return id;

            var sessionid = HttpContext.Session.GetInt32("UsuarioId") ?? HttpContext.Session.GetInt32("IdUsuario");
            if (sessionid.HasValue) return sessionid.Value;

            return 0;
        }
    }
}