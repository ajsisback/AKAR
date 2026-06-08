using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// A Saudi residential construction project owned by an Owner.
/// Acts as the project vault entry.
/// </summary>
public class Project : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public string ProjectName { get; private set; } = string.Empty;
    public ProjectType ProjectType { get; private set; }
    public string? City { get; private set; }
    public string? LocationText { get; private set; }
    public string? MapLink { get; private set; }
    public CurrentStage CurrentStage { get; private set; } = CurrentStage.NotStarted;
    public string? OptionalImageUrl { get; private set; }

    // Navigation
    public Owner Owner { get; private set; } = null!;

    private Project() { } // EF Core
    private Project(Guid id) : base(id) { }

    public static Project Create(
        Guid ownerId,
        string projectName,
        ProjectType projectType,
        string? city = null,
        string? locationText = null,
        string? mapLink = null,
        CurrentStage currentStage = CurrentStage.NotStarted,
        string? optionalImageUrl = null)
    {
        var project = new Project(Guid.NewGuid())
        {
            OwnerId = ownerId,
            ProjectName = projectName,
            ProjectType = projectType,
            City = city,
            LocationText = locationText,
            MapLink = mapLink,
            CurrentStage = currentStage,
            OptionalImageUrl = optionalImageUrl
        };

        return project;
    }

    public void UpdateStage(CurrentStage newStage)
    {
        CurrentStage = newStage;
        SetUpdatedAt();
    }

    public void UpdateSettings(string projectName, ProjectType projectType, string? city, string? locationText, string? mapLink)
    {
        ProjectName = projectName;
        ProjectType = projectType;
        City = city;
        LocationText = locationText;
        MapLink = mapLink;
        SetUpdatedAt();
    }
}
