using Akar.Application.Features.Followers;
using FluentValidation;

namespace Akar.Application.Validators;

public class CreateProjectFollowerCommandValidator : AbstractValidator<CreateProjectFollowerCommand>
{
    public CreateProjectFollowerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters");

        RuleFor(x => x.FollowerType)
            .NotEmpty().WithMessage("Follower type is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => x.Notes is not null);
    }
}

public class UpdateProjectFollowerCommandValidator : AbstractValidator<UpdateProjectFollowerCommand>
{
    public UpdateProjectFollowerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters");

        RuleFor(x => x.FollowerType)
            .NotEmpty().WithMessage("Follower type is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => x.Notes is not null);
    }
}
