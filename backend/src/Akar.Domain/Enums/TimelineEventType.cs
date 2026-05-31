namespace Akar.Domain.Enums;

/// <summary>
/// Types of timeline events for a project.
/// </summary>
public enum TimelineEventType
{
    StageChanged,
    ManualNote,
    FileUploaded,
    ContractCreated,
    ContractPdfGenerated,
    ContractSignedUploaded,
    FollowerAdded,
    FollowerFileUploaded
}
