using citi_core.Dto;
using FluentValidation;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .Length(2, 100).WithMessage("Full Name must be between 2 and 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{9,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be in valid E.164 format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept terms and conditions.");
    }
}
