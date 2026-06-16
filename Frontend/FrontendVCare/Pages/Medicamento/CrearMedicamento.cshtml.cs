using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FrontendVCare.Pages.Medicamento
{
    public class CrearMedicamentoModel : BasePageModel
    {
        private readonly MedicamentoAdapter _medicamentoAdapter;
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public CrearMedicamentoModel(
            MedicamentoAdapter medicamentoAdapter,
            ClasificacionAdapter clasificacionAdapter)
        {
            _medicamentoAdapter = medicamentoAdapter;
            _clasificacionAdapter = clasificacionAdapter;
        }

        [BindProperty]
        public MedicamentoDto Medicamento { get; set; } = new();

        public List<ClasificacionDto> Clasificaciones { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            Clasificaciones = await _clasificacionAdapter.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCrearMedicamentoAsync()
        {
            Clasificaciones = await _clasificacionAdapter.GetAllAsync();

            if (!ModelState.IsValid)
            {
                MensajeError = "Revisa los datos del medicamento. Hay campos obligatorios o con formato incorrecto.";
                return Page();
            }

            string? mensajeValidacion = ValidarMedicamento();
            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                MensajeError = mensajeValidacion;
                return Page();
            }

            int? idUsuario = ObtenerIdUsuarioSesion();
            if (idUsuario == null || idUsuario.Value == 0)
            {
                MensajeError = "No se encontró el usuario. Por favor, inicia sesión nuevamente.";
                return Page();
            }

            Medicamento.IdUsuario = idUsuario.Value;

            var (exito, mensaje) = await _medicamentoAdapter.CreateWithMessageAsync(Medicamento);
            if (exito)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { mensaje = "Medicamento creado correctamente" });
            }

            MensajeError = mensaje ?? "Error al crear el medicamento.";
            return Page();
        }

        private string? ValidarMedicamento()
        {
            if (string.IsNullOrWhiteSpace(Medicamento.Nombre))
                return "El nombre del medicamento es obligatorio.";

            if (!Regex.IsMatch(Medicamento.Nombre.Trim(), @"^[\p{L}0-9\s]+$"))
                return "El nombre del medicamento no debe contener signos ni caracteres especiales.";

            if (string.IsNullOrWhiteSpace(Medicamento.Presentacion))
                return "La presentación es obligatoria.";

            if (Medicamento.IdClasificacion <= 0)
                return "La clasificación es obligatoria.";

            if (string.IsNullOrWhiteSpace(Medicamento.Concentracion))
                return "La concentración es obligatoria.";

            if (Medicamento.Precio < 0)
                return "El precio no puede ser negativo.";

            if (Medicamento.Stock < 0)
                return "El stock no puede ser negativo.";

            return null;
        }
    }
}