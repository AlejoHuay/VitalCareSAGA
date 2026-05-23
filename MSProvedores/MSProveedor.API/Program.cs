using DotNetEnv;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Infraestructura.Repositorios;
using MSProveedor.Aplicacion.InputPorts;
using MSProveedor.Aplicacion.Interactors;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

// 1. Habilitar la carpeta Controllers
builder.Services.AddControllers(); 
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// =======================================================
// 2. INYECCIÓN DE DEPENDENCIAS (LA MAGIA DE LA ARQUITECTURA)
// =======================================================
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorInputPort, ProveedorInteractor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. Mapear los controladores
app.MapControllers(); 

app.Run();