using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Dto.Ventas;
using FrontendVCare.Pages.Base;
using Microsoft.AspNetCore.Http;
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

    public List<string> MetodosPago { get; set; } = new();

    public int? IdUsuarioActual { get; set; }

    [BindProperty]
    public VentaFormularioDto Venta { get; set; } = new();

    public string? MensajeError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        IdUsuarioActual = ObtenerIdUsuarioSesion();
        await CargarOpcionesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return acceso;

        int? idUsuario = ObtenerIdUsuarioSesion();
        IdUsuarioActual = idUsuario;

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

        if (string.IsNullOrWhiteSpace(Venta.MetodoPago))
        {
            MensajeError = "Selecciona el método de pago.";
            await CargarOpcionesAsync();
            return Page();
        }

        if (Venta.IdCliente <= 0 || Venta.Detalles.Count == 0)
        {
            MensajeError = "Selecciona un cliente y al menos un medicamento.";
            await CargarOpcionesAsync();
            return Page();
        }

        OperacionApiDto resultado = await ventaAdapter.RegistrarAsync(Venta);
        if (resultado.Exito)
            return RedirectToPage("/Ventas/Venta", new { mensaje = resultado.Mensaje });

        MensajeError = resultado.Mensaje;
        await CargarOpcionesAsync();
        return Page();
    }

    private async Task CargarOpcionesAsync()
    {
        MetodosPago = ObtenerMetodosPago();

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

    private static List<string> ObtenerMetodosPago()
    {
        return new List<string>
        {
            "Efectivo",
            "Tarjeta",
            "Transferencia"
        };
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

    public async Task<IActionResult> OnPostCrearClienteAsync()
    {
        IActionResult? acceso = ValidarAcceso("Admin", "Bioquimico");
        if (acceso != null)
            return new JsonResult(new { exito = false, mensaje = "Acceso denegado." }) { StatusCode = StatusCodes.Status403Forbidden };

        ClienteFormularioDto? cliente;
        try
        {
            cliente = await JsonSerializer.DeserializeAsync<ClienteFormularioDto>(Request.Body, JsonOptions);
        }
        catch
        {
            return BadRequest(new { exito = false, mensaje = "Datos de cliente inválidos." });
        }

        if (cliente == null)
            return BadRequest(new { exito = false, mensaje = "Datos de cliente inválidos." });

        if (cliente.IdUsuario == null || cliente.IdUsuario <= 0)
            return BadRequest(new { exito = false, mensaje = "Identificador de usuario inválido." });

        HttpResponseMessage response = await clienteAdapter.CrearConRespuestaAsync(cliente);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
            return BadRequest(new { exito = false, mensaje = error? ["mensaje"] ?? "No se pudo crear el cliente." });
        }

        var contenido = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        if (!contenido.TryGetProperty("data", out JsonElement dataElement))
            return BadRequest(new { exito = false, mensaje = "Cliente creado, pero no se devolvió información." });

        ClienteDto? clienteCreado = dataElement.Deserialize<ClienteDto>(JsonOptions);
        if (clienteCreado == null)
            return BadRequest(new { exito = false, mensaje = "Cliente creado, pero no se devolvió información válida." });

        return new JsonResult(new
        {
            exito = true,
            mensaje = "Cliente creado correctamente.",
            data = new
            {
                idCliente = clienteCreado.IdCliente,
                razonSocial = clienteCreado.RazonSocial,
                nit = clienteCreado.Nit
            }
        });
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
