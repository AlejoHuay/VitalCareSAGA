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
            AplicarConsumidorFinalSiCorresponde();

            string? errorFormulario = ValidarFormulario();
            if (errorFormulario != null)
            {
                MensajeError = errorFormulario;
                return Page();
            }

            OperacionApiDto resultado = await clienteApiAdapter.CrearAsync(Cliente);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Cliente", new { mensaje = resultado.Mensaje });
        }

        private void AplicarConsumidorFinalSiCorresponde()
        {
            if (!Cliente.EsConsumidorFinal)
                return;

            Cliente.Nit = "CF";
            Cliente.RazonSocial = "Consumidor Final";
            Cliente.CorreoElectronico = string.Empty;
        }

        private string? ValidarFormulario()
        {
            if (Cliente.EsConsumidorFinal)
                return null;

            if (string.IsNullOrWhiteSpace(Cliente.Nit))
                return "El NIT no puede estar vacío.";

            if (!Cliente.Nit.Trim().All(char.IsDigit))
                return "El NIT solo acepta números.";

            if (string.IsNullOrWhiteSpace(Cliente.RazonSocial))
                return "La razón social no puede estar vacía.";

            return null;
        }
    }
}
