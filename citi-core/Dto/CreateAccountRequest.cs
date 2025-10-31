using citi_core.Enums;
using System.ComponentModel.DataAnnotations;

namespace citi_core.Dto
{
    public class CreateAccountRequest
    {
        [Required]
        public AccountType AccountType { get; set; }

        [Required]
        [MaxLength(100)]
        public string Branch { get; set; } = default!;

        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        public decimal? InterestRate { get; set; }
        public int? TermMonths { get; set; }
        public DateTime? MaturityDate { get; set; }
    }
}