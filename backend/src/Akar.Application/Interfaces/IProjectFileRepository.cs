using Akar.Application.DTOs;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;

namespace Akar.Application.Interfaces;

public interface IProjectFileRepository
{
    Task<ProjectFile?> GetByIdForOwnerAsync(Guid fileId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFile>> GetActiveByFolderForOwnerAsync(Guid folderId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<List<ProjectFile>> GetDeletedByProjectForOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> CountActiveByFolderAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectFile file, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search and filter project files with pagination.
    /// Returns DTOs directly to avoid materializing navigation properties unnecessarily.
    /// </summary>
    Task<PagedResult<ProjectFileSearchResultDto>> SearchAsync(
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
        CancellationToken cancellationToken = default);
}
