using MSVentas.Dominio.Modelos;
using System.Data;

namespace MSVentas.Dominio.Puertos.PuertoSalida
{
    public interface IMedicamentoRepository : IRepository<Medicamento>
    {
        DataTable GetDestacados();
        int Count();
        int UpdateStock(int idMedicamento, int cantidad, bool esEntrada, int idUsuario);
    }
}
