using MSProductos.CasosDeUso.PuertosSalida;
using MSProductos.Infraestructura.Persistencia.Repositorios;
using MSProductos.Dominio.Entidades;

namespace MSProductos.Infraestructura.Creadores
{
    public class ClasificacionRepositoryCreator : RepositoryCreator<Clasificacion>
    {
        public override IRepository<Clasificacion> CreateRepo()
        {
            return new ClasificacionRepository();
        }
    }
}

