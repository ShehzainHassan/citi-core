namespace citi_core.Interfaces
{
    public interface IResult<T>
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        T? Value { get; }
        string? ErrorMessage { get; }
    }
}
