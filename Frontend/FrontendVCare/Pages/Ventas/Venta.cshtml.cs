using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas
{
    public class VentaModel : BasePageModel
    {
        private readonly VentaAdapter ventaAdapter;
        private readonly ClienteApiAdapter clienteAdapter;
        private readonly MedicamentoAdapter medicamentoAdapter;

        public VentaModel(
            VentaAdapter ventaAdapter,
            ClienteApiAdapter clienteAdapter,
            MedicamentoAdapter medicamentoAdapter)
        {
            this.ventaAdapter = ventaAdapter;
            this.clienteAdapter = clienteAdapter;
            this.medicamentoAdapter = medicamentoAdapter;
        }

        public List<VentaDto> Ventas { get; set; } = new();

        public List<ClienteDto> Clientes { get; set; } = new();

        public List<MedicamentoDto> Medicamentos { get; set; } = new();

        [BindProperty]
        public VentaFormularioDto Venta { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(string filtro = "", string? mensaje = null, string? error = null)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            Mensaje = mensaje;
            MensajeError = error;

            await CargarDatosAsync(filtro);
            return Page();
        }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            int? idUsuario = ObtenerIdUsuarioSesion();
            if (idUsuario == null || idUsuario.Value == 0)
                return RedirectToPage("/Ventas/Venta", new { error = "No se encontro el usuario. Inicia sesion nuevamente." });

            Venta.IdUsuario = idUsuario.Value;
            Venta.Detalles = Venta.Detalles
                .Where(d => d.IdMedicamento > 0 && d.Cantidad > 0)
                .ToList();

            if (Venta.IdCliente <= 0 || Venta.Detalles.Count == 0)
                return RedirectToPage("/Ventas/Venta", new { error = "Selecciona un cliente y al menos un medicamento." });

            OperacionApiDto resultado = await ventaAdapter.RegistrarAsync(Venta);
            return RedirectToPage("/Ventas/Venta", resultado.Exito
                ? new { mensaje = resultado.Mensaje }
                : new { error = resultado.Mensaje });
        }

        public async Task<IActionResult> OnPostAnularAsync(int id)
        {
            IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
            if (acceso != null)
                return acceso;

            int? idUsuario = ObtenerIdUsuarioSesion();
            if (idUsuario == null || idUsuario.Value == 0)
                return RedirectToPage("/Ventas/Venta", new { error = "No se encontro el usuario. Inicia sesion nuevamente." });

            OperacionApiDto resultado = await ventaAdapter.AnularAsync(id, idUsuario.Value);
            return RedirectToPage("/Ventas/Venta", resultado.Exito
                ? new { mensaje = resultado.Mensaje }
                : new { error = resultado.Mensaje });
        }

        private async Task CargarDatosAsync(string filtro)
        {
            try
            {
                Ventas = await ventaAdapter.ObtenerTodasAsync(filtro);
            }
            catch
            {
                MensajeError ??= "No se pudo cargar ventas. Verifica que MSVentas este disponible.";
            }

            try
            {
                Clientes = await clienteAdapter.ObtenerTodosAsync(string.Empty);
                if (Clientes.Count == 0)
                    Clientes = ObtenerClientesDePrueba();
            }
            catch
            {
                MensajeError ??= "No se pudo cargar clientes.";
                Clientes = ObtenerClientesDePrueba();
            }

            try
            {
                Medicamentos = await medicamentoAdapter.GetAllAsync();
                if (Medicamentos.Count == 0)
                    Medicamentos = ObtenerMedicamentosDePrueba();
            }
            catch
            {
                MensajeError ??= "No se pudo cargar medicamentos.";
                Medicamentos = ObtenerMedicamentosDePrueba();
            }
        }

        private static List<ClienteDto> ObtenerClientesDePrueba()
        {
            return new List<ClienteDto>
            {
                new ClienteDto
                {
                    IdCliente = 1,
                    RazonSocial = "Consumidor Final",
                    Nit = "0",
                    EsConsumidorFinal = true
                },
                new ClienteDto
                {
                    IdCliente = 2,
                    RazonSocial = "Farmacia Central SRL",
                    Nit = "10203040"
                },
                new ClienteDto
                {
                    IdCliente = 3,
                    RazonSocial = "Clinica San Rafael",
                    Nit = "99887766"
                }
            };
        }

        private static List<MedicamentoDto> ObtenerMedicamentosDePrueba()
        {
            return new List<MedicamentoDto>
            {
                new MedicamentoDto
                {
                    Id = 1,
                    Nombre = "Paracetamol",
                    Presentacion = "Tabletas",
                    Precio = 12.50m,
                    Stock = 40
                },
                new MedicamentoDto
                {
                    Id = 2,
                    Nombre = "Ibuprofeno",
                    Presentacion = "Capsulas",
                    Precio = 22.50m,
                    Stock = 24
                },
                new MedicamentoDto
                {
                    Id = 3,
                    Nombre = "Amoxicilina",
                    Presentacion = "Suspension",
                    Precio = 43.00m,
                    Stock = 18
                },
                new MedicamentoDto
                {
                    Id = 4,
                    Nombre = "Loratadina",
                    Presentacion = "Tabletas",
                    Precio = 16.00m,
                    Stock = 32
                }
            };
        }
    }
}
