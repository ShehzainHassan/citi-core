namespace citi_core.Dto
{
    public class UpdateBeneficiaryRequest
    {
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Nickname { get; set; } = string.Empty;
    }
}
