using citi_core.Dto;
using FluentValidation;

namespace citi_core.Validators
{
    public class CardStatusUpdateRequestValidator : AbstractValidator<CardStatusUpdateRequest>
    {
        public CardStatusUpdateRequestValidator()
        {
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
        }
    }
}
