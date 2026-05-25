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
    }
}
