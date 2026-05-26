using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Medicamento
{
    public class ActualizarMedicamentoModel : BasePageModel
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

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

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
                Medicamento = await _medicamentoAdapter.GetByIdAsync(Id);
                MensajeError = "Revisa los datos del medicamento. Hay campos obligatorios o con formato incorrecto.";
                return Page();
            }

            string? mensajeValidacion = ValidarMedicamento();
            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                Medicamento = await _medicamentoAdapter.GetByIdAsync(Id);
                MensajeError = mensajeValidacion;
                return Page();
            }

            int? idUsuario = ObtenerIdUsuarioSesion();
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

            var (exito, mensaje) = await _medicamentoAdapter.UpdateWithMessageAsync(medicamentoActualizado);
            if (exito)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { mensaje = "Medicamento actualizado correctamente" });
            }

            Medicamento = await _medicamentoAdapter.GetByIdAsync(Id);
            MensajeError = mensaje ?? "Error al actualizar el medicamento.";
            return Page();
        }

        private string? ValidarMedicamento()
        {
            if (Id <= 0)
                return "ID de medicamento inválido.";

            if (string.IsNullOrWhiteSpace(Nombre))
                return "El nombre del medicamento es obligatorio.";

            if (!Regex.IsMatch(Nombre.Trim(), @"^[\p{L}0-9\s]+$"))
                return "El nombre del medicamento no debe contener signos ni caracteres especiales.";

            if (IdClasificacion <= 0)
                return "La clasificación es obligatoria.";

            if (string.IsNullOrWhiteSpace(Concentracion))
                return "La concentración es obligatoria.";

            if (Precio < 0)
                return "El precio no puede ser negativo.";

            if (Stock < 0)
                return "El stock no puede ser negativo.";

            return null;
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
