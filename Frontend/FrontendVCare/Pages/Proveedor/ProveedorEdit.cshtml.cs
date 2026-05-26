using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Proveedor
{
    public class ProveedorEditModel : BasePageModel
    {
        private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);
        private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled);
        private static readonly Regex CorreoRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ProveedorApiAdapter _proveedorApiAdapter;

        [BindProperty]
        public ProveedorFormularioDto Proveedor { get; set; } = new();

        public ProveedorEditModel(ProveedorApiAdapter proveedorApiAdapter)
        {
            _proveedorApiAdapter = proveedorApiAdapter;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            ProveedorDto? proveedor = await _proveedorApiAdapter.ObtenerPorIdAsync(id);

            if (proveedor == null)
                return RedirectToPage("Proveedor", new { error = "Proveedor no encontrado." });

            Proveedor = new ProveedorFormularioDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                CorreoElectronico = proveedor.CorreoElectronico,
                Direccion = proveedor.Direccion,
                IdUsuario = proveedor.IdUsuario
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult? acceso = ValidarAccesoAdmin();
            if (acceso != null) return acceso;

            if (Proveedor.Id <= 0)
            {
                Estado.MensajeError = "ID de proveedor inválido.";
                return Page();
            }

            Proveedor.Nombre = Proveedor.Nombre?.Trim() ?? string.Empty;
            Proveedor.Telefono = Proveedor.Telefono?.Trim() ?? string.Empty; 
            Proveedor.CorreoElectronico = Proveedor.CorreoElectronico?.Trim() ?? string.Empty;
            Proveedor.Direccion = string.IsNullOrWhiteSpace(Proveedor.Direccion) ? null : Proveedor.Direccion.Trim();

            if (!ValidarProveedor())
                return Page();

            Proveedor.IdUsuario = ObtenerIdUsuarioActual();

            if (Proveedor.IdUsuario <= 0)
            {
                Estado.MensajeError = "Error de sesión: No se pudo identificar al usuario. Inicie sesión nuevamente.";
                return Page();
            }

            OperacionApiDto resultado = await _proveedorApiAdapter.ActualizarAsync(Proveedor.Id, Proveedor);

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
                Estado.MensajeError = "El nombre es un campo obligatorio.";
                return false;
            }

            if (!NombreRegex.IsMatch(Proveedor.Nombre))
            {
                Estado.MensajeError = "El nombre solo puede contener letras y espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Proveedor.CorreoElectronico))
            {
                Estado.MensajeError = "El correo electrónico es un campo obligatorio.";
                return false;
            }

            if (!CorreoRegex.IsMatch(Proveedor.CorreoElectronico))
            {
                Estado.MensajeError = "El formato del correo electrónico es inválido.";
                return false;
            }

            if (Proveedor.CorreoElectronico.EndsWith("@gmail", StringComparison.OrdinalIgnoreCase) || 
                Proveedor.CorreoElectronico.EndsWith("@hotmail", StringComparison.OrdinalIgnoreCase))
            {
                Estado.MensajeError = "El correo está incompleto. Asegúrese de incluir '.com'.";
                return false;
            }

            // Teléfono OBLIGATORIO
            if (string.IsNullOrWhiteSpace(Proveedor.Telefono))
            {
                Estado.MensajeError = "El teléfono es un campo obligatorio.";
                return false;
            }

            if (!TelefonoRegex.IsMatch(Proveedor.Telefono))
            {
                Estado.MensajeError = "El teléfono debe tener exactamente 8 dígitos.";
                return false;
            }

            return true;
        }

        private int ObtenerIdUsuarioActual()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (int.TryParse(claimId, out int id))
            {
                return id;
            }

            int? sessionId = ObtenerIdUsuarioSesion();
            if (sessionId.HasValue)
            {
                return sessionId.Value;
            }

            return 0;
        }
    }
}