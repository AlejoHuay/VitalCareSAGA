using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages
{
    public class ClienteCreateModel : BasePageModel
    {
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
            int? idUsuarioSesion = ObtenerIdUsuarioSesion();
            if (!idUsuarioSesion.HasValue)
                return RedirectToPage("/Auth/Login");

            Cliente.IdUsuario = idUsuarioSesion.Value;
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
