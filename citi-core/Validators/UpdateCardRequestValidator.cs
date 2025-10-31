using citi_core.Dto;
using FluentValidation;

namespace citi_core.Validators
{
    public class UpdateCardRequestValidator : AbstractValidator<UpdateCardRequest>
    {
        public UpdateCardRequestValidator()
        {
            RuleFor(x => x.CardName)
                .NotEmpty().MaximumLength(256);
            RuleFor(x => x.AccountId)
                .NotEmpty();
        }
    }
}
