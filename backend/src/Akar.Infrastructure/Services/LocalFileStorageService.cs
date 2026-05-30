using Akar.Shared.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Akar.Infrastructure.Services;

/// <summary>
/// Local file system implementation of IFileStorageService.
/// Stores files under: {LocalRootPath}/owners/{ownerId}/projects/{projectId}/folders/{folderId}/{storedFileName}
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _rootPath = configuration["Storage:LocalRootPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
    }

    public async Task<string> SaveAsync(Guid ownerId, Guid projectId, Guid folderId, string storedFileName, Stream content, CancellationToken cancellationToken = default)
    {
        ValidatePathComponent(storedFileName);

        var relativePath = BuildRelativePath(ownerId, projectId, folderId, storedFileName);
        var fullPath = Path.Combine(_rootPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("File saved: {RelativePath} ({Bytes} bytes)", relativePath, fileStream.Length);

        return relativePath;
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        ValidatePathComponent(storagePath);

        var fullPath = Path.Combine(_rootPath, storagePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found on disk: {StoragePath}", storagePath);
            return Task.FromResult<Stream?>(null);
        }

        // Verify the resolved path is still within root (path traversal prevention)
        var resolvedPath = Path.GetFullPath(fullPath);
        var resolvedRoot = Path.GetFullPath(_rootPath);
        if (!resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt detected: {StoragePath}", storagePath);
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        ValidatePathComponent(storagePath);

        var fullPath = Path.Combine(_rootPath, storagePath);

        var resolvedPath = Path.GetFullPath(fullPath);
        var resolvedRoot = Path.GetFullPath(_rootPath);
        if (!resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt on delete: {StoragePath}", storagePath);
            return Task.FromResult(false);
        }

        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        _logger.LogInformation("File physically deleted: {StoragePath}", storagePath);
        return Task.FromResult(true);
    }

    private static string BuildRelativePath(Guid ownerId, Guid projectId, Guid folderId, string storedFileName)
    {
        return Path.Combine(
            "owners", ownerId.ToString(),
            "projects", projectId.ToString(),
            "folders", folderId.ToString(),
            storedFileName);
    }

    private static void ValidatePathComponent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Path component cannot be empty.");

        if (value.Contains("..") || value.Contains('\0'))
            throw new ArgumentException("Invalid path component — path traversal not allowed.");
    }
}
