namespace MSProductos.Dominio.Validadores
{
    public interface IResult<T>
    {
        Result Validar(T entidad);
    }
}
