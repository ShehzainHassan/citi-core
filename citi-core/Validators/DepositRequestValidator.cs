using FluentValidation;
using citi_core.Dto;

namespace citi_core.Validators
{
    public class DepositRequestValidator : AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            RuleFor(x => x.ToAccountId)
                .NotEmpty().WithMessage("ToAccountId is required");

            RuleFor(x => x.Amount)
                .InclusiveBetween(0.01m, 1_000_000m).WithMessage("Amount must be between 0.01 and 1,000,000");

            RuleFor(x => x.Currency)
                .NotEmpty().MaximumLength(10);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.Source)
                .MaximumLength(100);
        }
    }
}