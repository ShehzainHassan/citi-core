using FluentValidation;
using citi_core.Dto;
using citi_core.Enums;

namespace citi_core.Validators
{
    public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(x => x.AccountType)
                .IsInEnum()
                .WithMessage("Invalid account type.");

            RuleFor(x => x.Branch)
                .NotEmpty()
                .WithMessage("Branch is required.")
                .MaximumLength(100);

            RuleFor(x => x.Currency)
                .MaximumLength(10)
                .When(x => !string.IsNullOrWhiteSpace(x.Currency));

            RuleFor(x => x.InterestRate)
                .InclusiveBetween(0, 100)
                .When(x => x.InterestRate.HasValue)
                .WithMessage("Interest rate must be between 0 and 100.");

            RuleFor(x => x.TermMonths)
                .InclusiveBetween(1, 600)
                .When(x => x.TermMonths.HasValue)
                .WithMessage("Term must be between 1 and 600 months.");

            RuleFor(x => x.MaturityDate)
                .GreaterThan(DateTime.UtcNow)
                .When(x => x.MaturityDate.HasValue)
                .WithMessage("Maturity date must be in the future.");

            When(x => x.AccountType == AccountType.TimeDeposit, () =>
            {
                RuleFor(x => x.TermMonths)
                    .NotNull()
                    .WithMessage("TermMonths is required for Time Deposit accounts.");

                RuleFor(x => x.MaturityDate)
                    .NotNull()
                    .WithMessage("MaturityDate is required for Time Deposit accounts.");
            });
        }
    }
}