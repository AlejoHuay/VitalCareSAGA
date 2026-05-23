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
            // Leer variables de entorno para MongoDB
            cadenaConexion = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                ?? throw new InvalidOperationException("No se encontro la cadena de conexion de MongoDB en variable MONGODB_CONNECTION_STRING.");
            nombreBaseDatos = Environment.GetEnvironmentVariable("MONGODB_DATABASE") 
                ?? throw new InvalidOperationException("No se encontro el nombre de la base de datos MongoDB en variable MONGODB_DATABASE.");
            nombreColeccionClientes = Environment.GetEnvironmentVariable("MONGODB_COLLECTION") 
                ?? throw new InvalidOperationException("No se encontro el nombre de la coleccion de clientes en variable MONGODB_COLLECTION.");
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
