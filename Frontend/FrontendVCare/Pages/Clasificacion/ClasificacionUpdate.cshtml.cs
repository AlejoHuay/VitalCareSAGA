using FrontendVCare.Dto.ClasificacionDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Adaptadores;

namespace FrontendVCare.Pages.Clasificacion
{
    public class ClasificacionUpdateModel : PageModel
    {
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public ClasificacionUpdateModel(ClasificacionAdapter clasificacionAdapter)
        {
            _clasificacionAdapter = clasificacionAdapter;
        }

        public ClasificacionDto? Clasificacion { get; set; }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public string Origen { get; set; } = string.Empty;

        [BindProperty]
        public string Descripcion { get; set; } = string.Empty;

        [TempData]
        public string? MensajeError { get; set; }

        // Cargar clasificación para edición
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Clasificacion = await _clasificacionAdapter.GetByIdAsync(id);

                if (Clasificacion == null)
                {
                    return RedirectToPage("Clasificacion", new { error = "Clasificación no encontrada" });
                }

                // Llenar las propiedades con los datos de la clasificación
                Id = Clasificacion.Id;
                Nombre = Clasificacion.Nombre;
                Origen = Clasificacion.Origen;
                Descripcion = Clasificacion.Descripcion;

                return Page();
            }
            catch
            {
                return RedirectToPage("Clasificacion", new { error = "Error al cargar la clasificación" });
            }
        }

        // Cargar clasificación para edición mediante POST
        public async Task<IActionResult> OnPostCargarClasificacionParaEdicionAsync(int id)
        {
            try
            {
                Clasificacion = await _clasificacionAdapter.GetByIdAsync(id);

                if (Clasificacion == null)
                {
                    return RedirectToPage("Clasificacion", new { error = "Clasificación no encontrada" });
                }

                // Llenar las propiedades con los datos de la clasificación
                Id = Clasificacion.Id;
                Nombre = Clasificacion.Nombre;
                Origen = Clasificacion.Origen;
                Descripcion = Clasificacion.Descripcion;

                return Page();
            }
            catch
            {
                return RedirectToPage("Clasificacion", new { error = "Error al cargar la clasificación" });
            }
        }

        // Actualizar clasificación
        public async Task<IActionResult> OnPostActualizarClasificacionAsync()
        {
            try
            {
                if (Id <= 0)
                {
                    MensajeError = "ID de clasificación inválido.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    MensajeError = "El nombre es requerido.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

                if (idUsuario == null || idUsuario == 0)
                {
                    MensajeError = "No se encontró el usuario. Por favor, inicia sesión nuevamente.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                var clasificacionActualizada = new ClasificacionDto
                {
                    Id = Id,
                    Nombre = Nombre,
                    Origen = Origen,
                    Descripcion = Descripcion,
                    IdUsuario = idUsuario.Value
                };

                var (exito, mensaje) = await _clasificacionAdapter.UpdateAsync(clasificacionActualizada);

                if (exito)
                {
                    return RedirectToPage("Clasificacion", new { mensaje = "Clasificación actualizada correctamente" });
                }
                else
                {
                    MensajeError = mensaje ?? "Error al actualizar la clasificación.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Ocurrió un error: {ex.Message}";
                Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                return Page();
            }
        }
    }
}
    