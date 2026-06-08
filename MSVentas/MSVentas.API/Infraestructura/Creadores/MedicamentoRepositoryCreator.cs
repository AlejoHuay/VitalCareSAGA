using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Persistencia.Repositorios;

namespace MSVentas.Infraestructura.Creadores
{
    public class MedicamentoRepositoryCreator : RepositoryCreator<Medicamento>
    {
        public override IRepository<Medicamento> CreateRepo()
        {
            return new MedicamentoRepository();
        }
    }
}
