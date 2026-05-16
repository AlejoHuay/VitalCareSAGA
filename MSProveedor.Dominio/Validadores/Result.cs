namespace MSProveedor.Dominio.Validadores;

public class Result<T> : IResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T Data { get; set; }

    public static Result<T> Exito(T data, string message = "Operación exitosa")
    {
        return new Result<T> { Success = true, Data = data, Message = message };
    }

    public static Result<T> Falla(string message)
    {
        return new Result<T> { Success = false, Data = default!, Message = message };
    }
}