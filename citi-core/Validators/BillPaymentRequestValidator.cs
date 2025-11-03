using FluentValidation;
using citi_core.Dto;

namespace citi_core.Validators
{
    public class BillPaymentRequestValidator : AbstractValidator<BillPaymentRequest>
    {
        public BillPaymentRequestValidator()
        {
            RuleFor(x => x.FromAccountId)
                .NotEmpty().WithMessage("FromAccountId is required");

            RuleFor(x => x.BillType)
                .IsInEnum().WithMessage("Invalid bill type");

            RuleFor(x => x.BillerName)
                .NotEmpty().WithMessage("BillerName is required")
                .MaximumLength(100);

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .MaximumLength(10);

            RuleFor(x => x.AccountReference)
                .NotEmpty().WithMessage("AccountReference is required")
                .MaximumLength(50);

            RuleFor(x => x.Amount)
                .InclusiveBetween(0.01m, 1_000_000m).WithMessage("Amount must be between 0.01 and 1,000,000");

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}