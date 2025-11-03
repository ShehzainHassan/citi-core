using FluentValidation;
using citi_core.Dto;

namespace citi_core.Validators
{
    public class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.FromAccountId)
                .NotEmpty().WithMessage("FromAccountId is required");

            RuleFor(x => x.ToAccountNumber)
                .NotEmpty().WithMessage("ToAccountNumber is required")
                .Matches(@"^[A-Z0-9]{10,20}$").WithMessage("ToAccountNumber must be 10–20 alphanumeric characters");

            RuleFor(x => x.BeneficiaryName)
                .MaximumLength(100).WithMessage("BeneficiaryName cannot exceed 100 characters");

            RuleFor(x => x.Amount)
                .InclusiveBetween(0.01m, 1_000_000m).WithMessage("Amount must be between 0.01 and 1,000,000");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be a valid 3-letter code");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.BeneficiaryNickname)
                .MaximumLength(100).WithMessage("BeneficiaryNickname cannot exceed 100 characters");
        }
    }
}