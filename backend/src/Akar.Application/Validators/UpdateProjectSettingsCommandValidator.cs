using FluentValidation;

namespace Akar.Application.Validators;

public class UpdateProjectSettingsCommandValidator : AbstractValidator<Features.ProjectSettings.UpdateProjectSettingsCommand>
{
    public UpdateProjectSettingsCommandValidator()
    {
        RuleFor(x => x.Request.ProjectName)
            .NotEmpty().WithMessage("PROJECT_NAME_REQUIRED")
            .MaximumLength(200);

        RuleFor(x => x.Request.ProjectType)
            .IsEnumName(typeof(Akar.Domain.Enums.ProjectType), caseSensitive: false)
            .WithMessage("INVALID_PROJECT_TYPE");

        RuleFor(x => x.Request.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Request.City));

        RuleFor(x => x.Request.LocationText)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Request.LocationText));

        RuleFor(v => v.Request.MapLink)
            .MaximumLength(500).WithMessage("MapLink must not exceed 500 characters.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("MapLink must be a valid URL.")
            .When(v => !string.IsNullOrEmpty(v.Request.MapLink));
    }
}
