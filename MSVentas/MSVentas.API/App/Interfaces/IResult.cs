using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IResult<T>
    {
        Result Validar(T entidad);
    }
}
