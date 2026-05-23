using DotNetEnv;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Aplicacion.Interactors;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Dominio.Validadores;
using MSProductos.Infraestructura.Repositorios;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Dependencias

builder.Services.AddScoped<IClasificacionRepository, ClasificacionRepository>();

builder.Services.AddScoped<IClasificacionInputPort, ClasificacionInteractor>();

builder.Services.AddScoped<IResult<Clasificacion>, ClasificacionValidador>();

builder.Services.AddScoped<IMedicamentoRepository, MedicamentoRepository>();

builder.Services.AddScoped<IMedicamentoInputPort, MedicamentoInteractor>();

builder.Services.AddScoped<IResult<Medicamento>, MedicamentoValidator>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();