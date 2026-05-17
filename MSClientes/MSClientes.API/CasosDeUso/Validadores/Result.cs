namespace MSClientes.API.CasosDeUso.Validadores
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        public Result(bool isSuccess, string error = "")
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok()
            => new Result(true);

        public static Result Fail(string error)
            => new Result(false, error);
    }
}
