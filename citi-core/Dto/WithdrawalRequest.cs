namespace citi_core.Dto
{
    public class WithdrawalRequest
    {
        public Guid FromAccountId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public string? Description { get; set; }

        public string? Reference { get; set; }
    }
}