namespace MSProveedor.Infraestructura.Conexion;

public class ConexionStringSingleton
{
    private static ConexionStringSingleton? _instancia;
    private static readonly object _bloqueo = new object();
    public string CadenaConexion { get; private set; }

    private ConexionStringSingleton()
    {
        // Leer variables de entorno para construir connection string PostgreSQL
        string host = Environment.GetEnvironmentVariable("POSTGRES_PROVEEDOR_HOST") ?? "localhost";
        string port = Environment.GetEnvironmentVariable("POSTGRES_PROVEEDOR_PORT") ?? "5432";
        string database = Environment.GetEnvironmentVariable("POSTGRES_PROVEEDOR_DATABASE") ?? "railway";
        string user = Environment.GetEnvironmentVariable("POSTGRES_PROVEEDOR_USER") ?? "postgres";
        string password = Environment.GetEnvironmentVariable("POSTGRES_PROVEEDOR_PASSWORD") ?? "";

        CadenaConexion = $"Host={host};Port={port};Database={database};Username={user};Password={password};";
    }

    public static ConexionStringSingleton Instancia
    {
        get
        {
            if (_instancia == null)
            {
                lock (_bloqueo)
                {
                    _instancia ??= new ConexionStringSingleton();
                }
            }

            return _instancia;
        }
    }
}
