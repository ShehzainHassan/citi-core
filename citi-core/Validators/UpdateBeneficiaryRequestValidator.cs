using FluentValidation;
using citi_core.Dto;

namespace citi_core.Validators
{
    public class UpdateBeneficiaryRequestValidator : AbstractValidator<UpdateBeneficiaryRequest>
    {
        public UpdateBeneficiaryRequestValidator()
        {
            RuleFor(x => x.BeneficiaryName)
                .NotEmpty().WithMessage("Beneficiary name is required")
                .MaximumLength(100);

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required")
                .MaximumLength(100);

            RuleFor(x => x.BankCode)
                .NotEmpty().WithMessage("Bank code is required")
                .MaximumLength(20);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .Matches(@"^\+?[0-9]*$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number must be numeric");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Nickname)
                .MaximumLength(50);
        }
    }
}