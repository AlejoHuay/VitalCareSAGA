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

        public string? Mensaje { get; set; }

        public string? MensajeError { get; set; }

        // Obtener lista de clasificaciones con filtro opcional
        public async Task OnGetAsync(string filtro = "", string? mensaje = null, string? error = null)
        {
            Mensaje = mensaje;
            MensajeError = error;

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
                int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

                if (idUsuario == null || idUsuario == 0)
                {
                    return RedirectToPage(new
                    {
                        error = "No se encontró la sesión del usuario."
                    });
                }

                bool exito = await _clasificacionAdapter.DeleteAsync(id, idUsuario.Value);

                if (exito)
                {
                    return RedirectToPage(new
                    {
                        mensaje = "Clasificación eliminada correctamente."
                    });
                }

                return RedirectToPage(new
                {
                    error = "Error al eliminar la clasificación."
                });
            }
            catch (Exception ex)
            {
                return RedirectToPage(new
                {
                    error = $"Ocurrió un error: {ex.Message}"
                });
            }
        }
    }
}
