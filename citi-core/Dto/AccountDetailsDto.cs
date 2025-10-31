using citi_core.Enums;

namespace citi_core.Dto
{
    public class AccountDetailsDto
    {
        public Guid AccountId { get; set; }
        public string MaskedAccountNumber { get; set; } = default!;
        public decimal Balance { get; set; }
        public string Branch { get; set; } = default!;
        public AccountStatus Status { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<CardDto> Cards { get; set; } = new();
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal AverageBalance { get; set; }

    }
}
