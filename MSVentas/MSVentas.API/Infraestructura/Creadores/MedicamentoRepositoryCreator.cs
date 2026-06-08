using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;

namespace ProyectoArqSoft.Infrastructure.Creadores
{
    public class MedicamentoRepositoryCreator : RepositoryCreator<Medicamento>
    {
        public override IRepository<Medicamento> CreateRepo()
        {
            return new MedicamentoRepository();
        }
    }
}
