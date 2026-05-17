namespace MSClientes.API.AdaptadoresDeInterfaz.Gateways
{
    public interface IRepository<T>
    {
        int Insert(T entidad);
        int Update(T entidad);
        int Delete(T entidad);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(string filtro);
        T? GetById(int id);
    }
}
