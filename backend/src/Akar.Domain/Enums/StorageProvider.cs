namespace Akar.Domain.Enums;

/// <summary>
/// Where the physical file binary is stored.
/// Sprint 2A stores metadata only — file upload comes later.
/// </summary>
public enum StorageProvider
{
    Local = 0,
    AzureBlob = 1,
    AmazonS3 = 2,
    GoogleCloudStorage = 3,
    AlibabaOss = 4
}
