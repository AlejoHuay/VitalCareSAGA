namespace MSClientes.API.CasosDeUso.Validadores
{
    public interface IResult<T>
    {
        Result Validar(T entidad);
    }
}
