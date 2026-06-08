using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using MediatR;

namespace Akar.Application.Features.ProjectSettings;

public record GetProjectSettingsQuery(Guid ProjectId, Guid OwnerId) : IRequest<ProjectSettingsDto>;

public class GetProjectSettingsQueryHandler : IRequestHandler<GetProjectSettingsQuery, ProjectSettingsDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectSettingsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectSettingsDto> Handle(GetProjectSettingsQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null || project.OwnerId != request.OwnerId)
        {
            throw new InvalidOperationException("PROJECT_NOT_FOUND");
        }

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
