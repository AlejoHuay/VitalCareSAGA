using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using System.Data;

namespace MSVentas.App.Interfaces
{
    public interface IMedicamentoService
    {
        DataTable ObtenerTodos();
        DataTable ObtenerTodos(string filtro);
        Medicamento? ObtenerPorId(int id);
        DataTable ObtenerDestacados();

        Result Crear(
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario);

        Result Actualizar(
            int id,
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario);

        Result EliminarLogicamente(int id, int idUsuario);

        Result UpdateStock(int idMedicamento, int cantidad, bool esEntrada, int idUsuario);   
    }
}
