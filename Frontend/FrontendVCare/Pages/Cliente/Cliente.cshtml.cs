using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages
{
    public class ClienteModel : PageModel
    {
        private const int IdUsuarioSistema = 1;
        private readonly ClienteApiAdapter clienteApiAdapter;

        public List<ClienteDto> Clientes { get; set; } = new();
        public string FiltroActual { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string MensajeError { get; set; } = string.Empty;

        public ClienteModel(ClienteApiAdapter clienteApiAdapter)
        {
            this.clienteApiAdapter = clienteApiAdapter;
        }

        public async Task OnGetAsync(string? filtro, string? mensaje, string? error)
        {
            FiltroActual = filtro?.Trim() ?? string.Empty;
            Mensaje = mensaje ?? string.Empty;
            MensajeError = error ?? string.Empty;

            try
            {
                Clientes = await clienteApiAdapter.ObtenerTodosAsync(FiltroActual);
            }
            catch (HttpRequestException)
            {
                MensajeError = "No se pudo cargar clientes. Verifica que MSClientes este ejecutandose y que la base de datos responda.";
                Clientes = new List<ClienteDto>();
            }
        }

        public async Task<IActionResult> OnPostEliminarClienteLogicamenteAsync(int id)
        {
            OperacionApiDto resultado = await clienteApiAdapter.EliminarAsync(id, IdUsuarioSistema);

            if (!resultado.Exito)
                return RedirectToPage("Cliente", new { error = resultado.Mensaje });

            return RedirectToPage("Cliente", new { mensaje = resultado.Mensaje });
        }
    }
}
