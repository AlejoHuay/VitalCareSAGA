using Microsoft.Extensions.Configuration;

namespace MSProductos.Infraestructura.Conexion
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
            IConfigurationRoot configuracion = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            cadenaConexion = configuracion.GetConnectionString("MySqlConnection")
                ?? throw new Exception(
                    "No se encontró la cadena de conexión 'MySqlConnection'."
                );
        }

        public string CadenaConexion
        {
            get { return cadenaConexion; }
        }
    }
}