using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages.Medicamento
{
    public class CrearMedicamentoModel : PageModel
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

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            Clasificaciones = await _clasificacionAdapter.GetAllAsync();
        }

        public async Task<IActionResult> OnPostCrearMedicamentoAsync()
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

            Medicamento.IdUsuario = idUsuario.Value;

            bool exito = await _medicamentoAdapter.CreateAsync(Medicamento);
            if (exito)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { mensaje = "Medicamento creado correctamente" });
            }

            MensajeError = "Error al crear el medicamento.";
            return Page();
        }
    }
}
