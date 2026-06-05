using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Validadores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.CasosDeUso.PuertosEntrada
{
    public interface IMedicamentoInputPort
    {
        IEnumerable<Medicamento> ObtenerTodos();
        IEnumerable<Medicamento> ObtenerTodos(string filtro);

        Medicamento? ObtenerPorId(int id);

        Result Crear(
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario
        );

        Result Actualizar(
            int id,
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario
        );

        Result EliminarLogicamente(int id, int idUsuario);
    }
}
