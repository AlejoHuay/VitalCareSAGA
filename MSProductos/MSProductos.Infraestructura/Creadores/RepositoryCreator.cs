using MSProductos.CasosDeUso.PuertosSalida;

namespace MSProductos.Infraestructura.Creadores
{
    public abstract class RepositoryCreator<T>
    {
        public abstract IRepository<T> CreateRepo();
    }
}
