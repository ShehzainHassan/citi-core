using FluentValidation;
using citi_core.Dto;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be in valid E.164 format.");

        RuleFor(x => x.Preferences)
            .NotNull().WithMessage("Preferences are required.")
            .SetValidator(new UserPreferencesDtoValidator());
    }
}