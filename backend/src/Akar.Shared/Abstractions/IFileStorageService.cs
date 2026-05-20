namespace Akar.Shared.Abstractions;

/// <summary>
/// Abstraction for file storage operations.
/// Sprint 1: Interface only — no implementation.
/// Keeps the architecture cloud-portable without Azure/AWS lock-in.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(string containerName, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    Task<string> GetUrlAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
}
