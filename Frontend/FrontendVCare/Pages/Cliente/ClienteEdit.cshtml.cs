using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages
{
    public class ClienteEditModel : BasePageModel
    {
        private readonly ClienteApiAdapter clienteApiAdapter;

        [BindProperty]
        public ClienteFormularioDto Cliente { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;

        public ClienteEditModel(ClienteApiAdapter clienteApiAdapter)
        {
            this.clienteApiAdapter = clienteApiAdapter;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            ClienteDto? cliente = await clienteApiAdapter.ObtenerPorIdAsync(id);

            if (cliente == null)
                return RedirectToPage("Cliente", new { error = "Cliente no encontrado." });

            CargarFormulario(cliente);
            return Page();
        }

        public async Task<IActionResult> OnPostActualizarClienteAsync()
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

            OperacionApiDto resultado = await clienteApiAdapter.ActualizarAsync(Cliente.IdCliente, Cliente);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            return RedirectToPage("Cliente", new { mensaje = resultado.Mensaje });
        }

        private void CargarFormulario(ClienteDto cliente)
        {
            Cliente = new ClienteFormularioDto
            {
                IdCliente = cliente.IdCliente,
                IdUsuario = cliente.IdUsuario,
                Estado = cliente.Estado,
                EsConsumidorFinal = cliente.EsConsumidorFinal ||
                    (cliente.Nit.Equals("CF", StringComparison.OrdinalIgnoreCase) &&
                     cliente.RazonSocial.Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase)),
                Nit = cliente.Nit,
                RazonSocial = cliente.RazonSocial,
                CorreoElectronico = cliente.CorreoElectronico
            };
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
