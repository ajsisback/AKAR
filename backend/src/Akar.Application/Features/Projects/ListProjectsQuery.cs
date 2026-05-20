using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Projects;

public record ListProjectsQuery(Guid OwnerId) : IRequest<Result<List<ProjectDto>>>;

public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, Result<List<ProjectDto>>>
{
    private readonly IProjectRepository _projectRepository;

    public ListProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<List<ProjectDto>>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        var dtos = projects.Select(p => new ProjectDto(
            p.Id, p.OwnerId, p.ProjectName,
            p.ProjectType.ToString(), p.City, p.LocationText,
            p.MapLink, p.CurrentStage.ToString(),
            p.OptionalImageUrl, p.CreatedAtUtc, p.UpdatedAtUtc))
            .ToList();

        return Result<List<ProjectDto>>.Success(dtos);
    }
}
