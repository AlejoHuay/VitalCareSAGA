using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.CasosDeUso.Interactores;
using MSReportes.API.CasosDeUso.PuertosEntrada;
using MSReportes.API.FrameworksYDrivers.Creadores;
using MSReportes.API.FrameworksYDrivers.Repositorios;
using MSReportes.API.CasosDeUso.Builders;

var builder = WebApplication.CreateBuilder(args);

// Servicios principales
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("MSVentas", client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSVENTAS_URL")
        ?? builder.Configuration["ApiUrls:MSVentas"]
        ?? "http://localhost:5200/";

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("MSProductos", client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSPRODUCTOS_URL")
        ?? builder.Configuration["ApiUrls:MSProductos"]
        ?? "http://localhost:5155/";

    client.BaseAddress = new Uri(baseUrl);
});

// Inyección de dependencias
builder.Services.AddScoped<IReporteVentasInputPort, ReporteVentasInteractor>();
builder.Services.AddScoped<IReporteVentasRepositorio, ReporteVentasRepositorio>();
builder.Services.AddScoped<IReporteVentasPdfCreador, ReporteVentasPorRolPdfCreador>();
builder.Services.AddScoped<IReporteVentasExcelCreador, ReporteVentasPorRolExcelCreador>();
builder.Services.AddScoped<IComprobanteVentaPdfCreador, ComprobanteVentaPdfCreador>();
builder.Services.AddScoped<IReporteVentasPorRolBuilder, ReporteVentasPorRolBuilder>();

// CORS para que el frontend pueda consumir el microservicio
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();
