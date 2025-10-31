namespace citi_core.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null, string? traceId = null) =>
            new() { Success = true, Data = data, Message = message, TraceId = traceId };

        public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null, string? traceId = null) =>
            new() { Success = false, Message = message, Errors = errors, TraceId = traceId };
    }
}
