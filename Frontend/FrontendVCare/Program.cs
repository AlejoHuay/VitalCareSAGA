using FrontendVCare.Adaptadores;
using FrontendVCare.Dto.ClasificacionDtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();


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