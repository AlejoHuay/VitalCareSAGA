namespace MSProductos.Infraestructura.Persistencia.Conexion
{
    public class ConexionStringSingleton
    {
        private static ConexionStringSingleton? instancia;
        private readonly string cadenaConexion;

        private static readonly object bloqueo = new object();

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
            // Leer variables de entorno para construir connection string MySQL
            string host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "ballast.proxy.rlwy.net";
            string port = Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "38879";
            string database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "railway";
            string user = Environment.GetEnvironmentVariable("MYSQL_USER") ?? "root";
            string password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "AkjRtHquDIjnFVTKwyfKYqiWBIrBYEBv";

            cadenaConexion = $"server={host};port={port};database={database};user={user};password={password};";
        }

        public string CadenaConexion
        {
            get { return cadenaConexion; }
        }
    }
}
