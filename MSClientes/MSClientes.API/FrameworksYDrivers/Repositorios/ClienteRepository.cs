using MSClientes.API.AdaptadoresDeInterfaz.Gateways;
using MSClientes.API.CasosDeUso.Utilidades;
using MSClientes.API.Entidades;
using MSClientes.API.FrameworksYDrivers.Persistencia.Conexion;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MSClientes.API.FrameworksYDrivers.Repositorios
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly IMongoCollection<Cliente> clientes;

        public ClienteRepository()
        {
            var configuracion = ConexionStringSingleton.Instancia;
            MongoClient clienteMongo = new MongoClient(configuracion.CadenaConexion);
            IMongoDatabase baseDatos = clienteMongo.GetDatabase(configuracion.NombreBaseDatos);
            clientes = baseDatos.GetCollection<Cliente>(configuracion.NombreColeccionClientes);
        }

        public int Insert(Cliente cliente)
        {
            if (cliente.IdCliente <= 0)
                cliente.IdCliente = ObtenerSiguienteId();

            cliente.FechaRegistro = DateTime.UtcNow;
            cliente.UltimaActualizacion = null;
            cliente.Estado = 1;

            clientes.InsertOne(cliente);
            return 1;
        }

        public int Update(Cliente cliente)
        {
            Cliente? clienteActual = GetById(cliente.IdCliente);
            if (clienteActual == null)
                return 0;

            cliente.FechaRegistro = clienteActual.FechaRegistro;
            cliente.UltimaActualizacion = DateTime.UtcNow;
            cliente.Estado = 1;

            FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq(c => c.IdCliente, cliente.IdCliente) &
                                               Builders<Cliente>.Filter.Eq(c => c.Estado, (short)1);

            ReplaceOneResult resultado = clientes.ReplaceOne(filtro, cliente);
            return resultado.ModifiedCount > 0 ? 1 : 0;
        }

        public int Delete(Cliente cliente)
        {
            FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq(c => c.IdCliente, cliente.IdCliente) &
                                               Builders<Cliente>.Filter.Eq(c => c.Estado, (short)1);
            UpdateDefinition<Cliente> actualizacion = Builders<Cliente>.Update
                .Set(c => c.Estado, (short)0)
                .Set(c => c.IdUsuario, cliente.IdUsuario)
                .Set(c => c.UltimaActualizacion, DateTime.UtcNow);

            UpdateResult resultado = clientes.UpdateOne(filtro, actualizacion);
            return resultado.ModifiedCount > 0 ? 1 : 0;
        }

        public IEnumerable<Cliente> GetAll()
        {
            return GetAll(string.Empty);
        }

        public IEnumerable<Cliente> GetAll(string filtro)
        {
            FilterDefinition<Cliente> filtroMongo = ConstruirFiltro(filtro);

            return clientes
                .Find(filtroMongo)
                .SortBy(c => c.RazonSocial)
                .ToList();
        }

        public Cliente? GetById(int id)
        {
            FilterDefinition<Cliente> filtro = Builders<Cliente>.Filter.Eq(c => c.IdCliente, id) &
                                               Builders<Cliente>.Filter.Eq(c => c.Estado, (short)1);

            return clientes.Find(filtro).FirstOrDefault();
        }

        public int Count()
        {
            return Convert.ToInt32(clientes.CountDocuments(c => c.Estado == 1));
        }

        private int ObtenerSiguienteId()
        {
            Cliente? ultimoCliente = clientes
                .Find(FilterDefinition<Cliente>.Empty)
                .SortByDescending(c => c.IdCliente)
                .FirstOrDefault();

            return ultimoCliente == null ? 1 : ultimoCliente.IdCliente + 1;
        }

        private static FilterDefinition<Cliente> ConstruirFiltro(string filtro)
        {
            FilterDefinition<Cliente> activos = Builders<Cliente>.Filter.Eq(c => c.Estado, (short)1);

            if (string.IsNullOrWhiteSpace(filtro))
                return activos;

            string texto = System.Text.RegularExpressions.Regex.Escape(StringHelper.QuitarEspacios(filtro));
            BsonRegularExpression expresion = new BsonRegularExpression(texto, "i");

            FilterDefinition<Cliente> busqueda = Builders<Cliente>.Filter.Regex(c => c.Nit, expresion) |
                                                 Builders<Cliente>.Filter.Regex(c => c.RazonSocial, expresion) |
                                                 Builders<Cliente>.Filter.Regex(c => c.CorreoElectronico, expresion);

            return activos & busqueda;
        }
    }
}
