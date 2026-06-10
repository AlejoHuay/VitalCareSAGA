var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

List<VentaDto> ventas =
[
    new VentaDto
    {
        Id = 1,
        Fecha = DateTime.Now.AddDays(-2),
        IdCliente = 1,
        Cliente = "Consumidor Final",
        IdUsuario = 1,
        Usuario = "admin",
        RolUsuario = "Admin",
        Total = 47.50m,
        Estado = "Registrada",
        Detalles =
        [
            new VentaDetalleDto
            {
                IdMedicamento = 1,
                Medicamento = "Paracetamol - Tabletas",
                Cantidad = 2,
                PrecioUnitario = 12.50m,
                Subtotal = 25.00m
            },
            new VentaDetalleDto
            {
                IdMedicamento = 2,
                Medicamento = "Ibuprofeno - Capsulas",
                Cantidad = 1,
                PrecioUnitario = 22.50m,
                Subtotal = 22.50m
            }
        ]
    },
    new VentaDto
    {
        Id = 2,
        Fecha = DateTime.Now.AddDays(-1),
        IdCliente = 2,
        Cliente = "Farmacia Central SRL",
        IdUsuario = 2,
        Usuario = "bioquimico",
        RolUsuario = "Bioquimico",
        Total = 86.00m,
        Estado = "Registrada",
        Detalles =
        [
            new VentaDetalleDto
            {
                IdMedicamento = 3,
                Medicamento = "Amoxicilina - Suspension",
                Cantidad = 2,
                PrecioUnitario = 43.00m,
                Subtotal = 86.00m
            }
        ]
    }
];

Dictionary<int, string> medicamentos = new()
{
    [1] = "Paracetamol - Tabletas",
    [2] = "Ibuprofeno - Capsulas",
    [3] = "Amoxicilina - Suspension",
    [4] = "Loratadina - Tabletas",
    [5] = "Omeprazol - Capsulas"
};

Dictionary<int, string> clientes = new()
{
    [1] = "Consumidor Final",
    [2] = "Farmacia Central SRL",
    [3] = "Clinica San Rafael"
};

app.MapGet("/api/ventas", (string? filtro) =>
{
    IEnumerable<VentaDto> resultado = ventas;

    if (!string.IsNullOrWhiteSpace(filtro))
    {
        resultado = resultado.Where(venta =>
            venta.Cliente.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
            venta.Usuario.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
            venta.Estado.Contains(filtro, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(resultado.OrderByDescending(venta => venta.Fecha));
});

app.MapPost("/api/ventas", (VentaFormularioDto request) =>
{
    if (request.IdCliente <= 0 || request.Detalles.Count == 0)
        return Results.BadRequest(new { mensaje = "Selecciona un cliente y al menos un medicamento." });

    List<VentaDetalleDto> detalles = request.Detalles
        .Where(detalle => detalle.IdMedicamento > 0 && detalle.Cantidad > 0)
        .Select(detalle =>
        {
            decimal precio = detalle.PrecioUnitario > 0 ? detalle.PrecioUnitario : 10m;

            return new VentaDetalleDto
            {
                IdMedicamento = detalle.IdMedicamento,
                Medicamento = medicamentos.GetValueOrDefault(detalle.IdMedicamento, $"Medicamento {detalle.IdMedicamento}"),
                Cantidad = detalle.Cantidad,
                PrecioUnitario = precio,
                Subtotal = detalle.Cantidad * precio
            };
        })
        .ToList();

    if (detalles.Count == 0)
        return Results.BadRequest(new { mensaje = "El detalle de venta no tiene medicamentos validos." });

    int id = ventas.Count == 0 ? 1 : ventas.Max(venta => venta.Id) + 1;
    string rol = request.IdUsuario == 1 ? "Admin" : "Bioquimico";

    VentaDto venta = new()
    {
        Id = id,
        Fecha = DateTime.Now,
        IdCliente = request.IdCliente,
        Cliente = clientes.GetValueOrDefault(request.IdCliente, $"Cliente {request.IdCliente}"),
        IdUsuario = request.IdUsuario,
        Usuario = request.IdUsuario == 1 ? "admin" : $"usuario-{request.IdUsuario}",
        RolUsuario = rol,
        Total = detalles.Sum(detalle => detalle.Subtotal),
        Estado = "Registrada",
        Detalles = detalles
    };

    ventas.Add(venta);

    return Results.Ok(new { mensaje = "Venta registrada correctamente.", venta });
});

app.MapDelete("/api/ventas/{id:int}", (int id, int idUsuario) =>
{
    VentaDto? venta = ventas.FirstOrDefault(item => item.Id == id);

    if (venta == null)
        return Results.NotFound(new { mensaje = "Venta no encontrada." });

    venta.Estado = "Anulada";
    return Results.Ok(new { mensaje = "Venta anulada correctamente." });
});

app.MapGet("/api/ventas/reporte-por-rol", (DateTime? desde, DateTime? hasta) =>
{
    IEnumerable<VentaDto> ventasReporte = ventas.Where(venta => venta.Estado != "Anulada");

    if (desde.HasValue)
        ventasReporte = ventasReporte.Where(venta => venta.Fecha.Date >= desde.Value.Date);

    if (hasta.HasValue)
        ventasReporte = ventasReporte.Where(venta => venta.Fecha.Date <= hasta.Value.Date);

    var reporte = ventasReporte
        .GroupBy(venta => venta.RolUsuario)
        .Select(grupo => new ReporteVentasPorRolDto
        {
            Rol = grupo.Key,
            CantidadVentas = grupo.Count(),
            TotalVentas = grupo.Sum(venta => venta.Total)
        })
        .OrderBy(item => item.Rol)
        .ToList();

    return Results.Ok(reporte);
});

app.Run();

public class VentaDto
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public int IdCliente { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public int IdUsuario { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string RolUsuario { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public List<VentaDetalleDto> Detalles { get; set; } = new();
}

public class VentaFormularioDto
{
    public int IdCliente { get; set; }

    public int IdUsuario { get; set; }

    public List<VentaDetalleFormularioDto> Detalles { get; set; } = new();
}

public class VentaDetalleDto
{
    public int IdMedicamento { get; set; }

    public string Medicamento { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }
}

public class VentaDetalleFormularioDto
{
    public int IdMedicamento { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}

public class ReporteVentasPorRolDto
{
    public string Rol { get; set; } = string.Empty;

    public int CantidadVentas { get; set; }

    public decimal TotalVentas { get; set; }
}
