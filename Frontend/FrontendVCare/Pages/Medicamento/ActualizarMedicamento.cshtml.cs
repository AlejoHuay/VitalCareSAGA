using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Medicamento
{
    public class ActualizarMedicamentoModel : PageModel
    {
        private readonly MedicamentoAdapter _medicamentoAdapter;
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public ActualizarMedicamentoModel(
            MedicamentoAdapter medicamentoAdapter,
            ClasificacionAdapter clasificacionAdapter)
        {
            _medicamentoAdapter = medicamentoAdapter;
            _clasificacionAdapter = clasificacionAdapter;
        }

        public MedicamentoDto? Medicamento { get; set; }
        public List<ClasificacionDto> Clasificaciones { get; set; } = new();

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public string Presentacion { get; set; } = string.Empty;

        [BindProperty]
        public int IdClasificacion { get; set; }

        [BindProperty]
        public string Concentracion { get; set; } = string.Empty;

        [BindProperty]
        public decimal Precio { get; set; }

        [BindProperty]
        public int Stock { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            return await CargarMedicamentoAsync(id);
        }

        public async Task<IActionResult> OnPostCargarMedicamentoParaEdicionAsync(int id)
        {
            return await CargarMedicamentoAsync(id);
        }

        public async Task<IActionResult> OnPostActualizarMedicamentoAsync()
        {
            Clasificaciones = await _clasificacionAdapter.GetAllAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuario == null || idUsuario.Value == 0)
            {
                MensajeError = "No se encontró el usuario. Por favor, inicia sesión nuevamente.";
                return Page();
            }

            var medicamentoActualizado = new MedicamentoDto
            {
                Id = Id,
                Nombre = Nombre,
                Presentacion = Presentacion,
                IdClasificacion = IdClasificacion,
                Concentracion = Concentracion,
                Precio = Precio,
                Stock = Stock,
                IdUsuario = idUsuario.Value
            };

            bool exito = await _medicamentoAdapter.UpdateAsync(medicamentoActualizado);
            if (exito)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { mensaje = "Medicamento actualizado correctamente" });
            }

            MensajeError = "Error al actualizar el medicamento.";
            return Page();
        }

        private async Task<IActionResult> CargarMedicamentoAsync(int id)
        {
            Medicamento = await _medicamentoAdapter.GetByIdAsync(id);
            if (Medicamento == null)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { error = "Medicamento no encontrado" });
            }

            Clasificaciones = await _clasificacionAdapter.GetAllAsync();
            Id = Medicamento.Id;
            Nombre = Medicamento.Nombre;
            Presentacion = Medicamento.Presentacion;
            IdClasificacion = Medicamento.IdClasificacion;
            Concentracion = Medicamento.Concentracion;
            Precio = Medicamento.Precio;
            Stock = Medicamento.Stock;

            return Page();
        }
    }
}
