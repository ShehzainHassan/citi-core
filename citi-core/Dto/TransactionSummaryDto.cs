namespace citi_core.Dto
{
    public class TransactionSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount { get; set; }
        public int TotalCount { get; set; }
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
    }

}
