using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using MediatR;

namespace Akar.Application.Features.ProjectSettings;

public record UpdateProjectSettingsCommand(
    Guid ProjectId,
    Guid OwnerId,
    UpdateProjectSettingsRequest Request) : IRequest<ProjectSettingsDto>;

public class UpdateProjectSettingsCommandHandler : IRequestHandler<UpdateProjectSettingsCommand, ProjectSettingsDto>
{
    private readonly IProjectRepository _projectRepository;

    public UpdateProjectSettingsCommandHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectSettingsDto> Handle(UpdateProjectSettingsCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null || project.OwnerId != request.OwnerId)
        {
            throw new InvalidOperationException("PROJECT_NOT_FOUND");
        }

        if (!Enum.TryParse<Akar.Domain.Enums.ProjectType>(request.Request.ProjectType, ignoreCase: true, out var projectType))
        {
            throw new InvalidOperationException("PROJECT_INVALID_TYPE");
        }

        project.UpdateSettings(
            request.Request.ProjectName,
            projectType,
            request.Request.City,
            request.Request.LocationText,
            request.Request.MapLink);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectSettingsDto(
            project.Id,
            project.ProjectName,
            project.ProjectType.ToString(),
            project.CurrentStage.ToString(),
            project.City,
            project.LocationText,
            project.MapLink,
            project.UpdatedAtUtc);
    }
}
