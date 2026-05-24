using Akar.Application.Features.Files;
using FluentValidation;

namespace Akar.Application.Validators;

public class UploadProjectFileCommandValidator : AbstractValidator<UploadProjectFileCommand>
{
    public UploadProjectFileCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("VALIDATION_PROJECT_ID_REQUIRED");

        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("VALIDATION_FOLDER_ID_REQUIRED");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("VALIDATION_OWNER_ID_REQUIRED");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("VALIDATION_FILE_NAME_REQUIRED")
            .MaximumLength(500).WithMessage("VALIDATION_FILE_NAME_TOO_LONG");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("VALIDATION_FILE_REQUIRED");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("VALIDATION_FILE_REQUIRED");
    }
}
