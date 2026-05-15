using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace MSUsuarios.Infraestructura.Persistencia.Conexion
{
    public class ConexionStringSingleton
    {
        private static ConexionStringSingleton? instancia;
        private static readonly object bloqueo = new object();
        private readonly string cadenaConexion;

        public static ConexionStringSingleton Instancia
        {
            get
            {
                if (instancia == null)
                {
                    lock (bloqueo)
                    {
                        instancia ??= new ConexionStringSingleton();
                    }
                }

                return instancia;
            }
        }

        private ConexionStringSingleton()
        {
            Env.Load("../.env");

            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            IConfigurationRoot configuracion = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.local.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            cadenaConexion = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                ?? configuracion.GetConnectionString("PostgresConnection")
                ?? ConstruirCadenaConexionDesdeVariables();
        }

        private static string ConstruirCadenaConexionDesdeVariables()
        {
            string host = ObtenerVariableObligatoria("POSTGRES_HOST");
            string port = ObtenerVariableObligatoria("POSTGRES_PORT");
            string database = ObtenerVariableObligatoria("POSTGRES_DB");
            string user = ObtenerVariableObligatoria("POSTGRES_USER");
            string password = ObtenerVariableObligatoria("POSTGRES_PASSWORD");

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};";
        }

        private static string ObtenerVariableObligatoria(string nombre)
        {
            string? valor = Environment.GetEnvironmentVariable(nombre);

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new Exception($"No se encontro la variable de entorno '{nombre}'.");
            }

            return valor;
        }

        public string CadenaConexion => cadenaConexion;
    }
}
