using citi_core.Dto;
using FluentValidation;

namespace citi_core.Validators
{
    public class VerifyOTPRequestValidator : AbstractValidator<VerifyOTPRequest>
    {
        public VerifyOTPRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(4, 6).WithMessage("OTP code must be between 4 and 6 digits.")
                .Matches("^[0-9]+$").WithMessage("OTP code must be numeric.");

            RuleFor(x => x.Purpose)
                .IsInEnum().WithMessage("Purpose must be a valid value.");
        }
    }
}
