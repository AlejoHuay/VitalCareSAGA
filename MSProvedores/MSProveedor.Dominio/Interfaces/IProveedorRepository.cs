using MSProveedor.Dominio.Entidades;

namespace MSProveedor.Dominio.Interfaces;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<bool> ExisteNombreAsync(string nombre);
    Task<IEnumerable<Proveedor>> ObtenerTodosAsync();
    Task<Proveedor?> ObtenerPorIdAsync(int id);
    Task<bool> ActualizarAsync(Proveedor proveedor);
    Task<bool> EliminarAsync(int id, int idUsuario); 
}