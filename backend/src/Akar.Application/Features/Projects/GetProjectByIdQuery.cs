using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Projects;

public record GetProjectByIdQuery(Guid ProjectId, Guid OwnerId) : IRequest<Result<ProjectDto>>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);

        if (project is null)
        {
            return Result<ProjectDto>.Failure("PROJECT_NOT_FOUND", "Project not found");
        }

        return Result<ProjectDto>.Success(new ProjectDto(
            project.Id, project.OwnerId, project.ProjectName,
            project.ProjectType.ToString(), project.City, project.LocationText,
            project.MapLink, project.CurrentStage.ToString(),
            project.OptionalImageUrl, project.CreatedAtUtc, project.UpdatedAtUtc));
    }
}
