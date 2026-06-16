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
        private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?: [A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)*$", RegexOptions.Compiled);
        private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled);
        private static readonly Regex CorreoRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EspaciosMultiplesRegex = new(@"\s+", RegexOptions.Compiled);

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

            NormalizarProveedor();

            if (!ValidarProveedor())
                return Page();

            Proveedor.IdUsuario = ObtenerIdUsuarioActual();

            if (Proveedor.IdUsuario <= 0)
            {
                Estado.MensajeError = "Error de sesión: No se pudo identificar al usuario. Inicie sesión nuevamente.";
                return Page();
            }

            OperacionApiDto resultado = await _proveedorApiAdapter.CrearAsync(Proveedor);

            if (!resultado.Exito)
            {
                if (EsMensajeExitoso(resultado.Mensaje))
                {
                    return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
                }

                Estado.MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
        }

        private void NormalizarProveedor()
        {
            Proveedor.Nombre = NormalizarTexto(Proveedor.Nombre);
            Proveedor.Telefono = Proveedor.Telefono?.Trim() ?? string.Empty;
            Proveedor.CorreoElectronico = Proveedor.CorreoElectronico?.Trim() ?? string.Empty;
            Proveedor.Direccion = string.IsNullOrWhiteSpace(Proveedor.Direccion)
                ? null
                : NormalizarTexto(Proveedor.Direccion);
        }

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return EspaciosMultiplesRegex.Replace(texto.Trim(), " ");
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
                Estado.MensajeError = "El nombre solo puede contener letras y un espacio entre palabras.";
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

        private static bool EsMensajeExitoso(string? mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                return false;

            return mensaje.Contains("creado", StringComparison.OrdinalIgnoreCase) ||
                   mensaje.Contains("registrado", StringComparison.OrdinalIgnoreCase) ||
                   mensaje.Contains("actualizado", StringComparison.OrdinalIgnoreCase) ||
                   mensaje.Contains("correctamente", StringComparison.OrdinalIgnoreCase);
        }

        private int ObtenerIdUsuarioActual()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (int.TryParse(claimId, out int id)) return id;

            int? sessionId = ObtenerIdUsuarioSesion();
            if (sessionId.HasValue) return sessionId.Value;

            return 0;
        }
    }
}