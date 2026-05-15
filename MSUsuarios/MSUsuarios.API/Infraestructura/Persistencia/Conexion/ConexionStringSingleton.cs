using DotNetEnv;

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
                        if (instancia == null)
                        {
                            instancia = new ConexionStringSingleton();
                        }
                    }
                }

                return instancia;
            }
        }

        private ConexionStringSingleton()
        {
            Env.Load("../.env");

            string? cadenaConexionCompleta = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
            if (!string.IsNullOrWhiteSpace(cadenaConexionCompleta))
            {
                cadenaConexion = cadenaConexionCompleta;
                return;
            }

            string host = ObtenerVariableObligatoria("POSTGRES_HOST");
            string port = ObtenerVariableObligatoria("POSTGRES_PORT");
            string database = ObtenerVariableObligatoria("POSTGRES_DB");
            string user = ObtenerVariableObligatoria("POSTGRES_USER");
            string password = ObtenerVariableObligatoria("POSTGRES_PASSWORD");

            cadenaConexion =
                $"Host={host};Port={port};Database={database};Username={user};Password={password};";
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
