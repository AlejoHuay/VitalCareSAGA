using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Validadores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Aplicacion.InputPorts
{
    public interface IClasificacionInputPort
    {
        IEnumerable<Clasificacion> ObtenerTodos();
        IEnumerable<Clasificacion> ObtenerTodos(string filtro);

        Clasificacion? ObtenerPorId(int id);

        Result Crear(string nombre, string origen, string descripcion, int idUsuario);

        Result Actualizar(int id, string nombre, string origen, string descripcion, int idUsuario);

        Result EliminarLogicamente(int id, int idUsuario);
    }
}
