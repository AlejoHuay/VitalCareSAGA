using FrontendVCare.Adaptadores;
using FrontendVCare.Adaptadores.Auth;
using FrontendVCare.Dto;
using FrontendVCare.Dto.ClasificacionDtos;
using FrontendVCare.Dto.MedicamentoDtos;
using FrontendVCare.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddHttpClient<ClienteApiAdapter>(client =>
{
    string baseUrl = builder.Configuration["ApiUrls:MSClientes"]
        ?? "http://localhost:5055/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<ClasificacionAdapter>(client =>
{
    string baseUrl = builder.Configuration["ApiUrls:MSProductos"]
        ?? "http://localhost:7141/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<ProveedorApiAdapter>(client =>
{
    string baseUrl = builder.Configuration["ApiUrls:MSProveedor"]
        ?? "http://localhost:5297/";
    client.BaseAddress = new Uri(baseUrl);
});

// Registrar HttpClient para AuthClient
builder.Services.AddHttpClient<AuthClient>(client =>
{
    string baseUrl = builder.Configuration["ApiUrls:MSAuth"]
        ?? "http://localhost:5086/";
    client.BaseAddress = new Uri(baseUrl);
});

// Registrar HttpClient para Usuarios
builder.Services.AddHttpClient<UsuarioAdapter>(client =>
{
    string baseUrl = builder.Configuration["ApiUrls:MSAuth"]
        ?? "http://localhost:5086/";
    client.BaseAddress = new Uri(baseUrl);
});

// Registrar adaptadores de Auth
builder.Services.AddScoped<LoginResponseAdapter>();
builder.Services.AddScoped<MensajeApiAdapter>();

// Registrar AdapterJSON para Clasificaciones
builder.Services.AddHttpClient<AdapterJSON<ClasificacionDto>>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:MSProductos"] ?? throw new InvalidOperationException("ApiUrls:ServicioVentas missing"));
});

// Registrar ClasificacionAdapter
builder.Services.AddScoped<ClasificacionAdapter>(sp =>
{
    var adapterJson = sp.GetRequiredService<AdapterJSON<ClasificacionDto>>();
    return new ClasificacionAdapter(adapterJson);
});

builder.Services.AddHttpClient<AdapterJSON<MedicamentoDto>>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:MSProductos"] ?? "http://localhost:7141/");
});

builder.Services.AddScoped<MedicamentoAdapter>(sp =>
{
    var adapterJson = sp.GetRequiredService<AdapterJSON<MedicamentoDto>>();
    return new MedicamentoAdapter(adapterJson);
});

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
