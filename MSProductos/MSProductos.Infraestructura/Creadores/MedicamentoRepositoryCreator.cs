using MSProductos.CasosDeUso.PuertosSalida;
using MSProductos.Infraestructura.Persistencia.Repositorios;
using MSProductos.Dominio.Entidades;

namespace MSProductos.Infraestructura.Creadores
{
    public class MedicamentoRepositoryCreator : RepositoryCreator<Medicamento>
    {
        public override IRepository<Medicamento> CreateRepo()
        {
            return new MedicamentoRepository();
        }
    }
}
