using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Persistencia.Repositorios;

namespace MSVentas.Infraestructura.Creadores
{
    public class VentaRepositoryCreator
    {
        public IVentaRepository CreateRepo()
        {
            return new VentaRepository();
        }
    }
}
