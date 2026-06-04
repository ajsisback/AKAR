using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

// ═══════════════════════════════════════════════════════════════
// Search project files query
// ═══════════════════════════════════════════════════════════════

public record SearchProjectFilesQuery(
    Guid ProjectId,
    Guid OwnerId,
    string? Q = null,
    Guid? FolderId = null,
    string? FileCategory = null,
    string? Extension = null,
    string? ContentType = null,
    DateTime? CreatedFromUtc = null,
    DateTime? CreatedToUtc = null,
    bool IncludeDeleted = false,
    string SortBy = "createdAtUtc",
    string SortDirection = "desc",
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ProjectFileSearchResultDto>>>;

// ═══════════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════════

public class SearchProjectFilesQueryHandler
    : IRequestHandler<SearchProjectFilesQuery, Result<PagedResult<ProjectFileSearchResultDto>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;

    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAtUtc", "originalFileName", "fileSizeBytes", "fileExtension"
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc", "desc"
    };

    public SearchProjectFilesQueryHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result<PagedResult<ProjectFileSearchResultDto>>> Handle(
        SearchProjectFilesQuery request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership (owner isolation)
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate pagination
        if (request.Page < 1)
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "INVALID_PAGE", "Page must be 1 or greater");

        if (request.PageSize < 1 || request.PageSize > 100)
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "INVALID_PAGE_SIZE", "Page size must be between 1 and 100");

        // 3. Validate sortBy
        if (!AllowedSortBy.Contains(request.SortBy))
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "INVALID_SORT_BY", $"Sort by must be one of: {string.Join(", ", AllowedSortBy)}");

        // 4. Validate sortDirection
        if (!AllowedSortDirection.Contains(request.SortDirection))
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "INVALID_SORT_DIRECTION", "Sort direction must be 'asc' or 'desc'");

        // 5. Validate date range
        if (request.CreatedFromUtc.HasValue && request.CreatedToUtc.HasValue
            && request.CreatedToUtc.Value < request.CreatedFromUtc.Value)
            return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                "INVALID_DATE_RANGE", "CreatedToUtc must be greater than or equal to CreatedFromUtc");

        // 6. Validate fileCategory if provided
        FileCategory? parsedCategory = null;
        if (!string.IsNullOrWhiteSpace(request.FileCategory))
        {
            if (!Enum.TryParse<FileCategory>(request.FileCategory, true, out var cat))
                return Result<PagedResult<ProjectFileSearchResultDto>>.Failure(
                    "INVALID_FILE_CATEGORY", $"File category must be one of: {string.Join(", ", Enum.GetNames<FileCategory>())}");
            parsedCategory = cat;
        }

        // 7. Normalize extension (accept "pdf" or ".pdf")
        var normalizedExtension = request.Extension;
        if (!string.IsNullOrWhiteSpace(normalizedExtension))
        {
            normalizedExtension = normalizedExtension.Trim();
            if (!normalizedExtension.StartsWith('.'))
                normalizedExtension = "." + normalizedExtension;
        }

        // 8. Execute search
        var result = await _fileRepository.SearchAsync(
            projectId: request.ProjectId,
            ownerId: request.OwnerId,
            searchTerm: request.Q?.Trim(),
            folderId: request.FolderId,
            fileCategory: parsedCategory,
            extension: normalizedExtension,
            contentType: request.ContentType?.Trim(),
            createdFromUtc: request.CreatedFromUtc,
            createdToUtc: request.CreatedToUtc,
            includeDeleted: request.IncludeDeleted,
            sortBy: request.SortBy,
            sortDescending: request.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase),
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return Result<PagedResult<ProjectFileSearchResultDto>>.Success(result);
    }
}
