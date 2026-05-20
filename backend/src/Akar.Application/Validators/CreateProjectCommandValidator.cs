using Akar.Application.Features.Projects;
using FluentValidation;

namespace Akar.Application.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("VALIDATION_PROJECT_NAME_REQUIRED")
            .MaximumLength(300).WithMessage("VALIDATION_PROJECT_NAME_TOO_LONG");

        RuleFor(x => x.ProjectType)
            .NotEmpty().WithMessage("VALIDATION_PROJECT_TYPE_REQUIRED");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("VALIDATION_CITY_TOO_LONG")
            .When(x => x.City is not null);

        RuleFor(x => x.LocationText)
            .MaximumLength(500).WithMessage("VALIDATION_LOCATION_TOO_LONG")
            .When(x => x.LocationText is not null);

        RuleFor(x => x.MapLink)
            .MaximumLength(2000).WithMessage("VALIDATION_MAP_LINK_TOO_LONG")
            .When(x => x.MapLink is not null);
    }
}
