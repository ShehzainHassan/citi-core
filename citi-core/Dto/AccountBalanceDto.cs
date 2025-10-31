namespace citi_core.Dto
{
    public class AccountBalanceDto
    {
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingAmount { get; set; }
    }
}