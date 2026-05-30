using Akar.Domain.Enums;

namespace Akar.Application.Services;

/// <summary>
/// Centralized file validation rules. Shared between owner upload and public follower upload.
/// </summary>
public static class FileValidationService
{
    public static readonly Dictionary<string, (FileCategory Category, long MaxBytes)> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Documents — 20 MB
            { ".pdf",  (FileCategory.Document, 20 * 1024 * 1024) },
            { ".doc",  (FileCategory.Document, 20 * 1024 * 1024) },
            { ".docx", (FileCategory.Document, 20 * 1024 * 1024) },
            { ".xls",  (FileCategory.Document, 20 * 1024 * 1024) },
            { ".xlsx", (FileCategory.Document, 20 * 1024 * 1024) },
            // Images — 10 MB
            { ".jpg",  (FileCategory.Image, 10 * 1024 * 1024) },
            { ".jpeg", (FileCategory.Image, 10 * 1024 * 1024) },
            { ".png",  (FileCategory.Image, 10 * 1024 * 1024) },
            { ".webp", (FileCategory.Image, 10 * 1024 * 1024) },
            // Videos — 100 MB
            { ".mp4",  (FileCategory.Video, 100 * 1024 * 1024) },
            { ".mov",  (FileCategory.Video, 100 * 1024 * 1024) },
        };

    public static readonly HashSet<string> BlockedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".ps1", ".sh", ".js", ".msi", ".dll", ".zip"
        };

    public static string[] GetAllowedExtensionNames() =>
        AllowedExtensions.Keys.Select(k => k.TrimStart('.')).ToArray();

    public static long GetMaxFileSizeBytes() =>
        AllowedExtensions.Values.Max(v => v.MaxBytes);

    /// <summary>Sanitizes a file name, removing path components and invalid characters.</summary>
    public static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            name = name.Replace(c, '_');
        }
        return string.IsNullOrWhiteSpace(name) ? "unnamed" : name;
    }
}
