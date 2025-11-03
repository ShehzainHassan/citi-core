namespace citi_core.Dto
{
    public class TransferRequest
    {
        public Guid FromAccountId { get; set; }
        public string ToAccountNumber { get; set; } = string.Empty;
        public string? BeneficiaryName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public bool SaveAsBeneficiary { get; set; } = false;
        public string? BeneficiaryNickname { get; set; }
    }
}