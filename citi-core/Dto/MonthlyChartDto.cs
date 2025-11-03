namespace citi_core.Dto
{
    public class MonthlyChartDto
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Net { get; set; }
        public int NormalizedIncome { get; set; }
        public int NormalizedExpenses { get; set; }
        public int NormalizedNet { get; set; }
    }

}
