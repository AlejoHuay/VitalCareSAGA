using DotNetEnv;
using Microsoft.OpenApi.Models;
using MSUsuarios.App.Interfaces;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Creadores;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables del archivo .env
Env.Load();

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServicioUsuarios API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT como: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("No se encontro Jwt:Key en la configuracion.");
string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("No se encontro Jwt:Issuer en la configuracion.");
string jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("No se encontro Jwt:Audience en la configuracion.");

Environment.SetEnvironmentVariable("JWT_KEY", jwtKey);
Environment.SetEnvironmentVariable("JWT_ISSUER", jwtIssuer);
Environment.SetEnvironmentVariable("JWT_AUDIENCE", jwtAudience);

// Registro de Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<TokenRepositoryCreator>();
builder.Services.AddScoped<ITokenRepository>(provider =>
{
    TokenRepositoryCreator creator = provider.GetRequiredService<TokenRepositoryCreator>();
    return creator.CreateRepo();
});

builder.Services.AddScoped<UsuarioTokenRepositoryCreator>();
builder.Services.AddScoped<IUsuarioTokenRepository>(provider =>
{
    UsuarioTokenRepositoryCreator creator = provider.GetRequiredService<UsuarioTokenRepositoryCreator>();
    return creator.CreateRepo();
});

// Registro de Validadores
builder.Services.AddScoped<UsuarioValidacionGeneral>();

// Registro de Servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioTokenService, UsuarioTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();