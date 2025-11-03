namespace citi_core.Dto
{
    public class TransactionReportResponse
    {
        public Dictionary<string, List<TransactionDto>> GroupedTransactions { get; set; }
        public TransactionSummaryDto Summary { get; set; }
        public List<MonthlyChartDto> Chart { get; set; }
    }

}
