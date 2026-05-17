using Microsoft.Extensions.Configuration;

namespace MSProveedor.Infraestructura.Conexion;

public class ConexionStringSingleton
{
    private static ConexionStringSingleton? _instancia;
    public string CadenaConexion { get; private set; }

    private ConexionStringSingleton()
    {
        // 1. Buscamos el archivo appsettings.json en el proyecto API
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuracion = builder.Build();

        // 2. Leemos la cadena de conexión por su nombre exacto
        CadenaConexion = configuracion.GetConnectionString("PostgresConnection") 
                         ?? throw new Exception("No se encontró la cadena de conexión en appsettings.json");
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