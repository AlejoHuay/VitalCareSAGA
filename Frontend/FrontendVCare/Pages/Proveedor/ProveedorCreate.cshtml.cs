using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Proveedor
{
    public class ProveedorCreateModel : PageModel
    {
        private const int IdUsuarioSistema = 1;
        private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);
        private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled);
        private readonly ProveedorApiAdapter proveedorApiAdapter;

        [BindProperty]
        public ProveedorFormularioDto Proveedor { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;

        public ProveedorCreateModel(ProveedorApiAdapter proveedorApiAdapter)
        {
            this.proveedorApiAdapter = proveedorApiAdapter;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCrearProveedorAsync()
        {
            if (!ValidarProveedor())
            {
                return Page();
            }

            Proveedor.Nombre = Proveedor.Nombre.Trim();
            Proveedor.Telefono = string.IsNullOrWhiteSpace(Proveedor.Telefono) ? null : Proveedor.Telefono.Trim();
            Proveedor.IdUsuario = IdUsuarioSistema;
            OperacionApiDto resultado = await proveedorApiAdapter.CrearAsync(Proveedor);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Proveedor", new { mensaje = resultado.Mensaje });
        }

        private bool ValidarProveedor()
        {
            if (string.IsNullOrWhiteSpace(Proveedor.Nombre))
            {
                MensajeError = "El nombre es requerido.";
                return false;
            }

            if (!NombreRegex.IsMatch(Proveedor.Nombre.Trim()))
            {
                MensajeError = "El nombre solo puede contener letras.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Proveedor.Telefono) && !TelefonoRegex.IsMatch(Proveedor.Telefono.Trim()))
            {
                MensajeError = "El telefono debe tener exactamente 8 digitos.";
                return false;
            }

            return true;
        }
    }
}
