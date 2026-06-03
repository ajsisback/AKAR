using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Infrastructure.Persistence;
using Akar.Shared;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class ProjectFileRepository : IProjectFileRepository
{
    private readonly AkarDbContext _context;

    public ProjectFileRepository(AkarDbContext context) => _context = context;

    public async Task<ProjectFile?> GetByIdForOwnerAsync(Guid fileId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.OwnerId == ownerId, cancellationToken);

    public async Task<List<ProjectFile>> GetActiveByFolderForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .Where(f => f.FolderId == folderId && f.OwnerId == ownerId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<List<ProjectFile>> GetDeletedByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId && f.IsDeleted)
            .OrderByDescending(f => f.DeletedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<int> CountActiveByFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles
            .CountAsync(f => f.FolderId == folderId && !f.IsDeleted, cancellationToken);

    public async Task AddAsync(ProjectFile file, CancellationToken cancellationToken = default)
        => await _context.ProjectFiles.AddAsync(file, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<PagedResult<ProjectFileSearchResultDto>> SearchAsync(
        Guid projectId,
        Guid ownerId,
        string? searchTerm = null,
        Guid? folderId = null,
        FileCategory? fileCategory = null,
        string? extension = null,
        string? contentType = null,
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null,
        bool includeDeleted = false,
        string sortBy = "createdAtUtc",
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Base query: owner-isolated, project-scoped
        var query = _context.ProjectFiles
            .Where(f => f.ProjectId == projectId && f.OwnerId == ownerId);

        // Deleted filter
        if (!includeDeleted)
            query = query.Where(f => !f.IsDeleted);

        // Search term — case-insensitive LIKE on OriginalFileName
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(f => EF.Functions.ILike(f.OriginalFileName, $"%{searchTerm}%"));

        // Folder filter
        if (folderId.HasValue)
            query = query.Where(f => f.FolderId == folderId.Value);

        // File category filter
        if (fileCategory.HasValue)
            query = query.Where(f => f.FileCategory == fileCategory.Value);

        // Extension filter (already normalized with leading dot by handler)
        if (!string.IsNullOrWhiteSpace(extension))
            query = query.Where(f => f.FileExtension.ToLower() == extension.ToLower());

        // Content type filter
        if (!string.IsNullOrWhiteSpace(contentType))
            query = query.Where(f => f.ContentType.ToLower() == contentType.ToLower());

        // Date range filters
        if (createdFromUtc.HasValue)
            query = query.Where(f => f.CreatedAtUtc >= createdFromUtc.Value);

        if (createdToUtc.HasValue)
            query = query.Where(f => f.CreatedAtUtc <= createdToUtc.Value);

        // Count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        IOrderedQueryable<ProjectFile> orderedQuery = sortBy.ToLowerInvariant() switch
        {
            "originalfilename" => sortDescending
                ? query.OrderByDescending(f => f.OriginalFileName)
                : query.OrderBy(f => f.OriginalFileName),
            "filesizebytes" => sortDescending
                ? query.OrderByDescending(f => f.FileSizeBytes)
                : query.OrderBy(f => f.FileSizeBytes),
            "fileextension" => sortDescending
                ? query.OrderByDescending(f => f.FileExtension)
                : query.OrderBy(f => f.FileExtension),
            _ => sortDescending // default: createdAtUtc
                ? query.OrderByDescending(f => f.CreatedAtUtc)
                : query.OrderBy(f => f.CreatedAtUtc)
        };

        // Pagination + projection (join with folders for FolderName)
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                _context.ProjectFolders,
                file => file.FolderId,
                folder => folder.Id,
                (file, folder) => new ProjectFileSearchResultDto(
                    file.Id,
                    file.ProjectId,
                    file.FolderId,
                    folder.FolderName,
                    file.OriginalFileName,
                    file.ContentType,
                    file.FileExtension,
                    file.FileSizeBytes,
                    file.FileCategory.ToString(),
                    file.IsDeleted,
                    file.DeletedAtUtc,
                    file.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectFileSearchResultDto>(items, totalCount, page, pageSize);
    }
}

