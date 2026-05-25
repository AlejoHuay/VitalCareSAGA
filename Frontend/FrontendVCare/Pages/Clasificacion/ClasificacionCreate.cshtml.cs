using FrontendVCare.Dto.ClasificacionDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Adaptadores;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Clasificacion
{
    public class ClasificacionCreateModel : PageModel
    {
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public ClasificacionCreateModel(ClasificacionAdapter clasificacionAdapter)
        {
            _clasificacionAdapter = clasificacionAdapter;
        }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public string Origen { get; set; } = string.Empty;

        [BindProperty]
        public string Descripcion { get; set; } = string.Empty;

        public string? MensajeError { get; set; }

        public void OnGet()
        {
        }

        // Crear clasificación
        public async Task<IActionResult> OnPostCrearClasificacionAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    MensajeError = "El nombre de la clasificación es obligatorio.";
                    return Page();
                }

                if (!Regex.IsMatch(Nombre.Trim(), @"^[\p{L}\s]+$"))
                {
                    MensajeError = "El nombre de la clasificación solo debe contener letras y espacios.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Origen))
                {
                    MensajeError = "El origen es obligatorio.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    MensajeError = "La descripción es obligatoria.";
                    return Page();
                }

                int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

                if (idUsuario == null || idUsuario == 0)
                {
                    MensajeError = "No se encontró el usuario. Por favor, inicia sesión nuevamente.";
                    return Page();
                }

                var nuevaClasificacion = new ClasificacionDto
                {
                    Nombre = Nombre,
                    Origen = Origen,
                    Descripcion = Descripcion,
                    IdUsuario = idUsuario.Value
                };

                var (exito, mensaje) = await _clasificacionAdapter.CreateAsync(nuevaClasificacion);

                if (exito)
                {
                    return RedirectToPage("Clasificacion", new { mensaje = "Clasificación creada correctamente" });
                }
                else
                {
                    MensajeError = mensaje ?? "Error al crear la clasificación.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Ocurrió un error: {ex.Message}";
                return Page();
            }
        }
    }
}
