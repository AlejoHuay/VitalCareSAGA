using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Proveedor
{
    public class ProveedorCreateModel : BasePageModel
    {
        private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);
        private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled);
        private readonly ProveedorApiAdapter _proveedorApiAdapter;

        [BindProperty]
        public ProveedorFormularioDto Proveedor { get; set; } = new();

        public ProveedorCreateModel(ProveedorApiAdapter proveedorApiAdapter)
        {
            _proveedorApiAdapter = proveedorApiAdapter;
        }

        public IActionResult OnGet()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            if (!ValidarProveedor())
                return Page();

            Proveedor.Nombre = Proveedor.Nombre.Trim();
            Proveedor.Telefono = string.IsNullOrWhiteSpace(Proveedor.Telefono) ? null : Proveedor.Telefono.Trim();
            
            Proveedor.IdUsuario = ObtenerIdUsuarioActual();

            if (Proveedor.IdUsuario <= 0)
            {
                Estado.MensajeError = "Error de sesión: No se pudo identificar al usuario. Inicie sesión nuevamente.";
                return Page();
            }

            OperacionApiDto resultado = await _proveedorApiAdapter.CrearAsync(Proveedor);

            if (!resultado.Exito)
            {
                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
        }

        private bool ValidarProveedor()
        {
            if (string.IsNullOrWhiteSpace(Proveedor.Nombre))
            {
                Estado.MensajeError = "El nombre es requerido.";
                return false;
            }

            if (!NombreRegex.IsMatch(Proveedor.Nombre.Trim()))
            {
                Estado.MensajeError = "El nombre solo puede contener letras.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Proveedor.Telefono) && !TelefonoRegex.IsMatch(Proveedor.Telefono.Trim()))
            {
                Estado.MensajeError = "El telefono debe tener exactamente 8 digitos.";
                return false;
            }

            return true;
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