using MSUsuarios.Dominio.Puertos.PuertoSalida;

namespace MSUsuarios.Infraestructura.Creadores
{
    public abstract class RepositoryCreator<T>
    {
        public abstract IRepository<T> CreateRepo();
    }
}
