namespace Akar.Shared.Abstractions;

/// <summary>
/// Abstraction for file storage operations.
/// Keeps the architecture cloud-portable without Azure/AWS lock-in.
/// Sprint 2B: LocalFileStorageService implementation.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file stream to storage.
    /// Returns the relative storage path (never an absolute local path).
    /// </summary>
    Task<string> SaveAsync(Guid ownerId, Guid projectId, Guid folderId, string storedFileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a read-only stream for the stored file.
    /// Returns null if the file does not exist.
    /// </summary>
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the physical file from storage.
    /// Not used in Sprint 2B (soft-delete only), reserved for future purge.
    /// </summary>
    Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
