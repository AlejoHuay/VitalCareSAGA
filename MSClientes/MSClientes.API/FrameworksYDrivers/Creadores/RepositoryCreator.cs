using MSClientes.API.AdaptadoresDeInterfaz.Gateways;

namespace MSClientes.API.FrameworksYDrivers.Creadores
{
    public abstract class RepositoryCreator<T>
    {
        public abstract IRepository<T> CreateRepo();
    }
}
