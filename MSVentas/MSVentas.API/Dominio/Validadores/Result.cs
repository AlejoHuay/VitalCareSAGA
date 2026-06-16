namespace MSVentas.Dominio.Validadores
{
    public class Result
    {
        public bool IsSuccess { get; }
        /// <summary>
        /// Use IsSuccess == false instead of IsFailure.
        /// </summary>
        public string Error { get; }
        public int? Id { get; }

        public Result(bool isSuccess, string error = "", int? id = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Id = id;
        }

        public static Result Ok(int? id = null)
            => new Result(true, id: id);

        public static Result Fail(string error)
            => new Result(false, error);
    }
}

