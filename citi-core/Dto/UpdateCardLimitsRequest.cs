using System.ComponentModel.DataAnnotations;

namespace citi_core.Dto
{
    public class UpdateCardLimitsRequest
    {
        [Range(0, 100000)]
        public decimal? DailyLimit { get; set; }

        [Range(0, 1000000)]
        public decimal? MonthlyLimit { get; set; }
    }

}
