using MSVentas.Dominio.Puertos.PuertoSalida;

namespace MSVentas.Infraestructura.Creadores
{
    public abstract class RepositoryCreator<T> //Clase creadora
    {
        public abstract IRepository<T> CreateRepo();

    }
}

