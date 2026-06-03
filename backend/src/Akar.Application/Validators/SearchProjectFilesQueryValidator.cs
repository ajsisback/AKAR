using Akar.Application.Features.Files;
using FluentValidation;

namespace Akar.Application.Validators;

public class SearchProjectFilesQueryValidator : AbstractValidator<SearchProjectFilesQuery>
{
    private static readonly string[] AllowedSortBy =
        ["createdAtUtc", "originalFileName", "fileSizeBytes", "fileExtension"];

    public SearchProjectFilesQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("VALIDATION_PROJECT_ID_REQUIRED");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("VALIDATION_OWNER_ID_REQUIRED");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("INVALID_PAGE");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("INVALID_PAGE_SIZE");

        RuleFor(x => x.SortBy)
            .Must(s => AllowedSortBy.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("INVALID_SORT_BY");

        RuleFor(x => x.SortDirection)
            .Must(s => s.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("INVALID_SORT_DIRECTION");

        RuleFor(x => x)
            .Must(x => !x.CreatedFromUtc.HasValue || !x.CreatedToUtc.HasValue ||
                       x.CreatedToUtc.Value >= x.CreatedFromUtc.Value)
            .WithMessage("INVALID_DATE_RANGE");
    }
}
