namespace MSClientes.API.FrameworksYDrivers.Persistencia.Conexion
{
    public class ConexionStringSingleton
    {
        private static ConexionStringSingleton? instancia;
        private static readonly object bloqueo = new object();
        private readonly string cadenaConexion;
        private readonly string nombreBaseDatos;
        private readonly string nombreColeccionClientes;

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
            IConfigurationRoot configuracion = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            cadenaConexion = configuracion["MongoDb:ConnectionString"]
                ?? throw new InvalidOperationException("No se encontro la cadena de conexion de MongoDB.");
            nombreBaseDatos = configuracion["MongoDb:DatabaseName"]
                ?? throw new InvalidOperationException("No se encontro el nombre de la base de datos MongoDB.");
            nombreColeccionClientes = configuracion["MongoDb:ClientesCollectionName"]
                ?? throw new InvalidOperationException("No se encontro el nombre de la coleccion de clientes.");
        }

        public string CadenaConexion
        {
            get { return cadenaConexion; }
        }

        public string NombreBaseDatos
        {
            get { return nombreBaseDatos; }
        }

        public string NombreColeccionClientes
        {
            get { return nombreColeccionClientes; }
        }
    }
}
