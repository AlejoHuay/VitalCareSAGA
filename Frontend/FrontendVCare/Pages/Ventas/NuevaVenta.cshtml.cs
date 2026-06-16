using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Mvc;

namespace FrontendVCare.Pages.Ventas;

public class NuevaVentaModel : BasePageModel
{
    private readonly VentaAdapter ventaAdapter;
    private readonly ClienteApiAdapter clienteAdapter;
    private readonly MedicamentoAdapter medicamentoAdapter;

    public NuevaVentaModel(
        VentaAdapter ventaAdapter,
        ClienteApiAdapter clienteAdapter,
        MedicamentoAdapter medicamentoAdapter)
    {
        this.ventaAdapter = ventaAdapter;
        this.clienteAdapter = clienteAdapter;
        this.medicamentoAdapter = medicamentoAdapter;
    }

    public List<ClienteDto> Clientes { get; set; } = new();

    public List<MedicamentoDto> Medicamentos { get; set; } = new();

    [BindProperty]
    public VentaFormularioDto Venta { get; set; } = new();

    public string? MensajeError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        await CargarOpcionesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        int? idUsuario = ObtenerIdUsuarioSesion();
        if (idUsuario == null || idUsuario.Value == 0)
        {
            MensajeError = "No se encontro el usuario. Inicia sesion nuevamente.";
            await CargarOpcionesAsync();
            return Page();
        }

        Venta.IdUsuario = idUsuario.Value;
        Venta.Detalles = Venta.Detalles
            .Where(detalle => detalle.IdMedicamento > 0 && detalle.Cantidad > 0)
            .ToList();

        if (Venta.IdCliente <= 0 || Venta.Detalles.Count == 0)
        {
            MensajeError = "Selecciona un cliente y al menos un medicamento.";
            await CargarOpcionesAsync();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Venta.MetodoPago))
        {
            MensajeError = "Debe seleccionar un metodo de pago.";
            await CargarOpcionesAsync();
            return Page();
        }

        if (!EsMetodoPagoValido(Venta.MetodoPago))
        {
            MensajeError = "El metodo de pago seleccionado no es valido.";
            await CargarOpcionesAsync();
            return Page();
        }

        ClienteDto? cliente = await ObtenerClienteSeleccionadoAsync(Venta.IdCliente);
        if (cliente == null)
        {
            MensajeError = "No se pudo obtener los datos del cliente seleccionado.";
            await CargarOpcionesAsync();
            return Page();
        }

        Venta.Nit = cliente.Nit;
        Venta.RazonSocial = cliente.RazonSocial;

        OperacionApiDto resultado = await ventaAdapter.RegistrarAsync(Venta);
        if (resultado.Exito)
        {
            if (resultado.Id.HasValue && resultado.Id.Value > 0)
            {
                return RedirectToPage("/Ventas/VerVenta", new
                {
                    id = resultado.Id.Value,
                    descargarComprobante = true
                });
            }

            return RedirectToPage("/Ventas/Venta", new { mensaje = resultado.Mensaje });
        }

        MensajeError = resultado.Mensaje;
        await CargarOpcionesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearClienteRapidoAsync(
        string clienteModalNit,
        string clienteModalRazonSocial,
        string? clienteModalCorreoElectronico)
    {
        int? idUsuario = ObtenerIdUsuarioSesion();
        if (idUsuario == null || idUsuario.Value == 0)
        {
            return new JsonResult(new
            {
                success = false,
                error = "No se encontro el usuario. Inicia sesion nuevamente."
            })
            { StatusCode = StatusCodes.Status401Unauthorized };
        }

        clienteModalNit = clienteModalNit?.Trim() ?? string.Empty;
        clienteModalRazonSocial = clienteModalRazonSocial?.Trim() ?? string.Empty;
        clienteModalCorreoElectronico = clienteModalCorreoElectronico?.Trim();

        if (string.IsNullOrWhiteSpace(clienteModalNit) ||
            string.IsNullOrWhiteSpace(clienteModalRazonSocial))
        {
            return new JsonResult(new
            {
                success = false,
                error = "Debe ingresar NIT / CI y razon social."
            })
            { StatusCode = StatusCodes.Status400BadRequest };
        }

        try
        {
            List<ClienteDto> existentes = await clienteAdapter.ObtenerTodosAsync(clienteModalNit);
            ClienteDto? existente = existentes.FirstOrDefault(cliente =>
                cliente.Nit.Equals(clienteModalNit, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
                return CrearRespuestaCliente(existente);

            ClienteFormularioDto clienteFormulario = new()
            {
                Nit = clienteModalNit,
                RazonSocial = clienteModalRazonSocial,
                CorreoElectronico = clienteModalCorreoElectronico ?? string.Empty,
                IdUsuario = idUsuario.Value
            };

            OperacionApiDto resultado = await clienteAdapter.CrearAsync(clienteFormulario);
            if (!resultado.Exito)
            {
                return new JsonResult(new
                {
                    success = false,
                    error = resultado.Mensaje ?? "No se pudo registrar el cliente."
                })
                { StatusCode = StatusCodes.Status400BadRequest };
            }

            List<ClienteDto> actualizados = await clienteAdapter.ObtenerTodosAsync(clienteModalNit);
            ClienteDto? creado = actualizados.FirstOrDefault(cliente =>
                cliente.Nit.Equals(clienteModalNit, StringComparison.OrdinalIgnoreCase));

            if (creado == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    error = "Cliente creado, pero no se pudo recuperar para seleccionarlo."
                })
                { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CrearRespuestaCliente(creado);
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                success = false,
                error = ex.Message
            })
            { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private async Task CargarOpcionesAsync()
    {
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

    private async Task<ClienteDto?> ObtenerClienteSeleccionadoAsync(int idCliente)
    {
        ClienteDto? cliente = Clientes.FirstOrDefault(c => c.IdCliente == idCliente);
        if (cliente != null)
            return cliente;

        try
        {
            return await clienteAdapter.ObtenerPorIdAsync(idCliente);
        }
        catch
        {
            return null;
        }
    }

    private static bool EsMetodoPagoValido(string metodoPago)
    {
        return metodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase)
            || metodoPago.Equals("QR", StringComparison.OrdinalIgnoreCase);
    }

    private static JsonResult CrearRespuestaCliente(ClienteDto cliente)
    {
        return new JsonResult(new
        {
            success = true,
            cliente = new
            {
                id = cliente.IdCliente,
                nit = cliente.Nit,
                razonSocial = cliente.RazonSocial,
                esConsumidorFinal = cliente.EsConsumidorFinal
            }
        });
    }

    private static List<ClienteDto> ObtenerClientesDePrueba()
    {
        return
        [
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
        ];
    }

    private static List<MedicamentoDto> ObtenerMedicamentosDePrueba()
    {
        return
        [
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
        ];
    }
}
