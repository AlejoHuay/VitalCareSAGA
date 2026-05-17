using MSClientes.API.Entidades;

namespace MSClientes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        int Count();
    }
}
