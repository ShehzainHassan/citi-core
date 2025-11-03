namespace citi_core.Dto
{
    public class DepositRequest
    {
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public string? Source { get; set; }
        public string? Reference { get; set; }
    }
}