using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;

namespace ProyectoArqSoft.Infrastructure.Creadores
{
    public class VentaRepositoryCreator
    {
        public IVentaRepository CreateRepo()
        {
            return new VentaRepository();
        }
    }
}
