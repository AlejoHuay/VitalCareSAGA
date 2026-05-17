using Microsoft.Extensions.Configuration;

namespace MSProveedor.Infraestructura.Conexion;

public class ConexionStringSingleton
{
    private static ConexionStringSingleton? _instancia;
    public string CadenaConexion { get; private set; }

    private ConexionStringSingleton()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        IConfiguration configuracion = builder.Build();

        CadenaConexion = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                         ?? configuracion.GetConnectionString("PostgresConnection")
                         ?? throw new Exception("No se encontro la cadena de conexion 'PostgresConnection' ni la variable 'POSTGRES_CONNECTION_STRING'.");
    }

    public static ConexionStringSingleton Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = new ConexionStringSingleton();
            }

            return _instancia;
        }
    }
}
