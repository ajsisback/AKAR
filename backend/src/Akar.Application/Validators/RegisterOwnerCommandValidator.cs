using Akar.Application.Features.Auth;
using FluentValidation;

namespace Akar.Application.Validators;

public class RegisterOwnerCommandValidator : AbstractValidator<RegisterOwnerCommand>
{
    public RegisterOwnerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("VALIDATION_FULLNAME_REQUIRED")
            .MaximumLength(200).WithMessage("VALIDATION_FULLNAME_TOO_LONG");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("VALIDATION_EMAIL_REQUIRED")
            .EmailAddress().WithMessage("VALIDATION_EMAIL_INVALID");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("VALIDATION_PHONE_REQUIRED")
            .MaximumLength(20).WithMessage("VALIDATION_PHONE_TOO_LONG");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("VALIDATION_PASSWORD_REQUIRED")
            .MinimumLength(8).WithMessage("VALIDATION_PASSWORD_TOO_SHORT");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("VALIDATION_COMPANY_TOO_LONG")
            .When(x => x.CompanyName is not null);
    }
}
