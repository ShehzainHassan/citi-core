namespace citi_core.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Details { get; set; }
    }

}
