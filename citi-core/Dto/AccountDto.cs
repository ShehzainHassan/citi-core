using citi_core.Enums;

namespace citi_core.Dto
{
    public class AccountDto
    {
        public Guid AccountId { get; set; }
        public string MaskedAccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Branch { get; set; } = string.Empty;
        public AccountStatus Status { get; set; }
    }

}
