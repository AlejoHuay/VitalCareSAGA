using MSVentas.Dominio.Validadores;

namespace MSVentas.App.Interfaces
{
    public interface IResult<T>
    {
        Result Validar(T entidad);
    }
}
