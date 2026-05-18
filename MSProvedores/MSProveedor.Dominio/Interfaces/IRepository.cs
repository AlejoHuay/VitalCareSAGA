namespace MSProveedor.Dominio.Interfaces;

public interface IRepository<T> where T : class
{
    Task<int> CrearAsync(T entidad);
}