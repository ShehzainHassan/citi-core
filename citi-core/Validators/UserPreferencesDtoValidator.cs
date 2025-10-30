using FluentValidation;
using citi_core.Dto;

public class UserPreferencesDtoValidator : AbstractValidator<UserPreferencesDto>
{
    public UserPreferencesDtoValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Length(2, 10);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(2, 5);

        RuleFor(x => x.Theme)
            .IsInEnum().WithMessage("Invalid theme selected.");

        RuleFor(x => x.NotificationsEnabled)
            .NotNull().WithMessage("NotificationsEnabled must be specified.");
    }
}