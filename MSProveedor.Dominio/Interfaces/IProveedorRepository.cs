using MSProveedor.Dominio.Entidades;

namespace MSProveedor.Dominio.Interfaces;

public interface IProveedorRepository : IRepository<Proveedor>
{
    // Aquí puedes agregar métodos específicos de Proveedor que no estén en IRepository
    Task<bool> ExisteNombreAsync(string nombre);
}