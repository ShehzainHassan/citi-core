namespace citi_core.Dto
{
    public class CardLimitsResponseDto
    {
        public decimal? DailyLimit { get; set; }
        public decimal? MonthlyLimit { get; set; }
        public decimal DailyUsage { get; set; }
        public decimal MonthlyUsage { get; set; }
    }

}
