using FluentValidation;

namespace Akar.Application.Validators;

public class UpdateOwnerProfileCommandValidator : AbstractValidator<Features.OwnerProfile.UpdateOwnerProfileCommand>
{
    public UpdateOwnerProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("PROFILE_NAME_REQUIRED")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
