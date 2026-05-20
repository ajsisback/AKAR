using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Projects;

public record CreateProjectCommand(
    Guid OwnerId,
    string ProjectName,
    string ProjectType,
    string? City,
    string? LocationText,
    string? MapLink,
    string? CurrentStage,
    string? OptionalImageUrl) : IRequest<Result<ProjectDto>>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectCommandHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProjectType>(request.ProjectType, ignoreCase: true, out var projectType))
        {
            return Result<ProjectDto>.Failure("PROJECT_INVALID_TYPE", "Invalid project type");
        }

        var stage = Domain.Enums.CurrentStage.NotStarted;
        if (!string.IsNullOrWhiteSpace(request.CurrentStage))
        {
            if (!Enum.TryParse<Domain.Enums.CurrentStage>(request.CurrentStage, ignoreCase: true, out stage))
            {
                return Result<ProjectDto>.Failure("PROJECT_INVALID_STAGE", "Invalid construction stage");
            }
        }

        var project = Project.Create(
            request.OwnerId,
            request.ProjectName,
            projectType,
            request.City,
            request.LocationText,
            request.MapLink,
            stage,
            request.OptionalImageUrl);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(MapToDto(project));
    }

    private static ProjectDto MapToDto(Project p) => new(
        p.Id, p.OwnerId, p.ProjectName,
        p.ProjectType.ToString(), p.City, p.LocationText,
        p.MapLink, p.CurrentStage.ToString(),
        p.OptionalImageUrl, p.CreatedAtUtc, p.UpdatedAtUtc);
}
