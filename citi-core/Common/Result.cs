using citi_core.Interfaces;

namespace citi_core.Common
{
    namespace citi_core.Common
    {
        public class Result<T>
        {
            public bool IsSuccess { get; set; }
            public string? ErrorMessage { get; set; }
            public T? Value { get; set; }
            public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
            public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, ErrorMessage = error };
        }
    }


}
