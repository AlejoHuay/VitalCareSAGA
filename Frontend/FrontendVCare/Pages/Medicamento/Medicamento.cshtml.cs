using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Medicamento
{
    public class MedicamentoPageModel : BasePageModel
    {
        private readonly MedicamentoAdapter _medicamentoAdapter;
        private readonly ClasificacionAdapter _clasificacionAdapter;

        public MedicamentoPageModel(
            MedicamentoAdapter medicamentoAdapter,
            ClasificacionAdapter clasificacionAdapter)
        {
            _medicamentoAdapter = medicamentoAdapter;
            _clasificacionAdapter = clasificacionAdapter;
        }

        public List<MedicamentoDto> Medicamentos { get; set; } = new();
        public List<ClasificacionDto> Clasificaciones { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(string filtro = "", string? mensaje = null, string? error = null)
        {
            // Valida que el usuario tenga rol Admin o Bioquimico
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            Mensaje = mensaje;
            MensajeError = error;

            Medicamentos = await _medicamentoAdapter.GetAllAsync();
            Clasificaciones = await _clasificacionAdapter.GetAllAsync();
            CompletarNombresDeClasificacion();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                Medicamentos = Medicamentos
                    .Where(m =>
                        (!string.IsNullOrEmpty(m.Nombre) && m.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.Presentacion) && m.Presentacion.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.Clasificacion) && m.Clasificacion.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            return Page();
        }

        private void CompletarNombresDeClasificacion()
        {
            Dictionary<int, string> nombresPorId = Clasificaciones
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.First().Nombre);

            foreach (MedicamentoDto medicamento in Medicamentos)
            {
                if (!string.IsNullOrWhiteSpace(medicamento.Clasificacion))
                    continue;

                if (nombresPorId.TryGetValue(medicamento.IdClasificacion, out string? nombreClasificacion))
                    medicamento.Clasificacion = nombreClasificacion;
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            int? idUsuario = ObtenerIdUsuarioSesion();
            if (idUsuario == null || idUsuario.Value == 0)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { error = "No se encontró el usuario. Por favor, inicia sesión nuevamente." });
            }

            bool exito = await _medicamentoAdapter.DeleteAsync(id, idUsuario.Value);

            if (exito)
            {
                return RedirectToPage("/Medicamento/Medicamento", new { mensaje = "Medicamento eliminado correctamente" });
            }

            return RedirectToPage("/Medicamento/Medicamento", new { error = "Error al eliminar el medicamento" });
        }
    }
}
