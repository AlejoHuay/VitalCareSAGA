using MSClientes.API.AdaptadoresDeInterfaz.Gateways;
using MSClientes.API.Entidades;
using MSClientes.API.FrameworksYDrivers.Repositorios;

namespace MSClientes.API.FrameworksYDrivers.Creadores
{
    public class ClienteRepositoryCreator : RepositoryCreator<Cliente>
    {
        public override IRepository<Cliente> CreateRepo()
        {
            return new ClienteRepository();
        }

        public IClienteRepository CreateClienteRepo()
        {
            return new ClienteRepository();
        }
    }
}
