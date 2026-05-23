using DotNetEnv;
using MSClientes.API.AdaptadoresDeInterfaz.Gateways;
using MSClientes.API.CasosDeUso.Interactores;
using MSClientes.API.CasosDeUso.PuertosEntrada;
using MSClientes.API.CasosDeUso.Validadores;
using MSClientes.API.Entidades;
using MSClientes.API.FrameworksYDrivers.Creadores;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ClienteRepositoryCreator>();
builder.Services.AddScoped<IClienteRepository>(provider =>
{
    var creator = provider.GetRequiredService<ClienteRepositoryCreator>();
    return creator.CreateClienteRepo();
});
builder.Services.AddScoped<IResult<Cliente>, ClienteValidacion>();
builder.Services.AddScoped<IClienteInputPort, ClienteInteractor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:5081")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
