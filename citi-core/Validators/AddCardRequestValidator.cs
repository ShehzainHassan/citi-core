namespace citi_core.Validators
{
    using citi_core.Dto;
    using citi_core.Enums;
    using FluentValidation;

    public class AddCardRequestValidator : AbstractValidator<AddCardRequest>
    {
        public AddCardRequestValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("AccountId is required");

            RuleFor(x => x.CardType)
                .IsInEnum().WithMessage("Invalid card type");

            RuleFor(x => x.CardHolderName)
                .NotEmpty().WithMessage("CardHolderName is required")
                .MaximumLength(256).WithMessage("CardHolderName must not exceed 256 characters");

            RuleFor(x => x.CardName)
                .NotEmpty().WithMessage("CardName is required")
                .MaximumLength(256).WithMessage("CardName must not exceed 256 characters");

            When(x => x.CardType == CardType.Credit, () =>
            {
                RuleFor(x => x.DesiredCreditLimit)
                    .NotNull().WithMessage("DesiredCreditLimit is required for credit cards")
                    .GreaterThan(0).WithMessage("DesiredCreditLimit must be greater than zero");
            });

            When(x => x.CardType == CardType.Debit, () =>
            {
                RuleFor(x => x.DesiredCreditLimit)
                    .Null().WithMessage("DesiredCreditLimit must not be provided for debit cards");
            });
        }
    }
}
