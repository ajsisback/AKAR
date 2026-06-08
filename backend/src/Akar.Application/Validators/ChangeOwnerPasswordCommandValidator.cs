using FluentValidation;

namespace Akar.Application.Validators;

public class ChangeOwnerPasswordCommandValidator : AbstractValidator<Features.OwnerProfile.ChangeOwnerPasswordCommand>
{
    public ChangeOwnerPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("PASSWORD_TOO_WEAK")
            .Matches("[A-Z]").WithMessage("PASSWORD_TOO_WEAK")
            .Matches("[a-z]").WithMessage("PASSWORD_TOO_WEAK")
            .Matches("[0-9]").WithMessage("PASSWORD_TOO_WEAK")
            .Matches("[^a-zA-Z0-9]").WithMessage("PASSWORD_TOO_WEAK");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword).WithMessage("PASSWORD_CONFIRMATION_MISMATCH");
    }
}
