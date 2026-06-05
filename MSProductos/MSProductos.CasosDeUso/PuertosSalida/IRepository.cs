using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.CasosDeUso.PuertosSalida
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
