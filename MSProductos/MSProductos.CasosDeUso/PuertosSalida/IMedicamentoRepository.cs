using MSProductos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.CasosDeUso.PuertosSalida
{
    public interface IMedicamentoRepository : IRepository<Medicamento>
    {
        bool ExisteNombreActivo(string nombre);
        bool ExisteNombreActivoExcluyendoId(int idMedicamento, string nombre);
    }
}
