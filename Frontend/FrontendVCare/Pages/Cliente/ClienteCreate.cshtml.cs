using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendVCare.Pages
{
    public class ClienteCreateModel : PageModel
    {
        private const int IdUsuarioSistema = 1;
        private readonly ClienteApiAdapter clienteApiAdapter;

        [BindProperty]
        public ClienteFormularioDto Cliente { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;

        public ClienteCreateModel(ClienteApiAdapter clienteApiAdapter)
        {
            this.clienteApiAdapter = clienteApiAdapter;
        }

        public void OnGet()
        {
            Cliente.EsConsumidorFinal = false;
        }

        public async Task<IActionResult> OnPostCrearClienteAsync()
        {
            Cliente.IdUsuario = IdUsuarioSistema;
            OperacionApiDto resultado = await clienteApiAdapter.CrearAsync(Cliente);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Cliente", new { mensaje = resultado.Mensaje });
        }
    }
}
