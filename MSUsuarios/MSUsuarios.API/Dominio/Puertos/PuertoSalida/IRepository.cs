namespace MSUsuarios.Dominio.Puertos.PuertoSalida
{
    public interface IRepository<T>
    {
        int Insert(T t);
        int Update(T t);
        int Delete(T t);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(string filtro);
        T? GetById(int id);
    }
}
