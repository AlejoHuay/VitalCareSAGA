using FrontendVCare.Dto.ClasificacionDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendVCare.Adaptadores;

namespace FrontendVCare.Pages.Clasificacion
{
    public class ClasificacionModel : PageModel
    {
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public ClasificacionModel(ClasificacionAdapter clasificacionAdapter)
        {
            _clasificacionAdapter = clasificacionAdapter;
        }

        public List<ClasificacionDto> Clasificaciones { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        // Obtener lista de clasificaciones con filtro opcional
        public async Task OnGetAsync(string filtro = "")
        {
            try
            {
                Clasificaciones = await _clasificacionAdapter.GetAllAsync();

                if (!string.IsNullOrEmpty(filtro))
                {
                    Clasificaciones = Clasificaciones
                        .Where(c =>
                            (!string.IsNullOrEmpty(c.Nombre) && c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(c.Origen) && c.Origen.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(c.Descripcion) && c.Descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                        )
                        .ToList();
                }
            }
            catch
            {
                MensajeError = "No se pudieron cargar las clasificaciones.";
            }
        }

        // Eliminar una clasificación por id
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                bool exito = await _clasificacionAdapter.DeleteAsync(id);

                if (exito)
                {
                    Mensaje = "Clasificación eliminada correctamente.";
                }
                else
                {
                    MensajeError = "Error al eliminar la clasificación.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Ocurrió un error: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
