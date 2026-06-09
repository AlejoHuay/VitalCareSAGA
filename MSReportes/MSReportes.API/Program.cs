using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.CasosDeUso.Interactores;
using MSReportes.API.CasosDeUso.PuertosEntrada;
using MSReportes.API.FrameworksYDrivers.Creadores;
using MSReportes.API.FrameworksYDrivers.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Servicios principales
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Inyección de dependencias
builder.Services.AddScoped<IReporteVentasInputPort, ReporteVentasInteractor>();
builder.Services.AddScoped<IReporteVentasRepositorio, ReporteVentasRepositorio>();
builder.Services.AddScoped<IReporteVentasPdfCreador, ReporteVentasPorRolPdfCreador>();
builder.Services.AddScoped<IReporteVentasExcelCreador, ReporteVentasPorRolExcelCreador>();

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