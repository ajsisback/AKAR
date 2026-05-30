namespace Akar.Domain.Enums;

/// <summary>
/// Source type for timeline events, indicating which entity triggered the event.
/// </summary>
public enum TimelineSourceType
{
    None,
    Project,
    ProjectFile,
    ProjectContract,
    ProjectFollower,
    FollowerUploadLink
}
