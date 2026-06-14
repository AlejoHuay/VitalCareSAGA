using DotNetEnv;
using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Auth;
using FrontendVCare.Adaptadores.Ventas;
using FrontendVCare.Dto;
using FrontendVCare.Servicios;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Registrar el manejador de tokens JWT
builder.Services.AddScoped<JwtTokenHandler>();

builder.Services.AddHttpClient<ClienteApiAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSCLIENTES_URL") 
        ?? builder.Configuration["ApiUrls:MSClientes"]
        ?? "http://localhost:5055/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ClasificacionAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSPRODUCTOS_URL")
        ?? builder.Configuration["ApiUrls:MSProductos"]
        ?? "http://localhost:5141/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<ProveedorApiAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSPROVEEDOR_URL") 
        ?? builder.Configuration["ApiUrls:MSProveedor"]
        ?? "http://localhost:5297/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Registrar HttpClient para AuthClient
builder.Services.AddHttpClient<AuthClient>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSUSUARIOS_URL")
        ?? builder.Configuration["ApiUrls:MSAuth"]
        ?? "http://localhost:5281/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Registrar HttpClient para Usuarios
builder.Services.AddHttpClient<UsuarioAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSUSUARIOS_URL")
        ?? builder.Configuration["ApiUrls:MSAuth"]
        ?? "http://localhost:5281/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddHttpClient<VentaAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSVENTAS_URL")
        ?? builder.Configuration["ApiUrls:MSVentas"]
        ?? "http://localhost:5069/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

// Registrar adaptadores de Auth
builder.Services.AddScoped<LoginResponseAdapter>();
builder.Services.AddScoped<MensajeApiAdapter>();

builder.Services.AddHttpClient<MedicamentoAdapter>(client =>
{
    string baseUrl = Environment.GetEnvironmentVariable("MSPRODUCTOS_URL")
        ?? builder.Configuration["ApiUrls:MSProductos"]
        ?? "http://localhost:5141/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<JwtTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
