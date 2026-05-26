using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using FrontendVCare.Adaptadores;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Clasificacion
{
    public class ClasificacionUpdateModel : BasePageModel
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

        public string? MensajeError { get; set; }

        // Cargar clasificación para edición
        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

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
                    MensajeError = "El nombre de la clasificación es obligatorio.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                if (!Regex.IsMatch(Nombre.Trim(), @"^[\p{L}\s]+$"))
                {
                    MensajeError = "El nombre de la clasificación solo debe contener letras y espacios.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Origen))
                {
                    MensajeError = "El origen es obligatorio.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    MensajeError = "La descripción es obligatoria.";
                    Clasificacion = await _clasificacionAdapter.GetByIdAsync(Id);
                    return Page();
                }

                int? idUsuario = ObtenerIdUsuarioSesion();

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
    
