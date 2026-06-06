using MSProductos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Dominio.Interfaces
{
    public interface IClasificacionRepository : IRepository<Clasificacion>
    {
        bool TieneMedicamentosActivosAsociados(int idClasificacion);
        bool ExisteNombreActivo(string nombre);
        bool ExisteNombreActivoExcluyendoId(int idClasificacion, string nombre);
    }
}
