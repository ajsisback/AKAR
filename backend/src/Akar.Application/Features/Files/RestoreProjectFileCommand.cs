using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

public record RestoreProjectFileCommand(
    Guid ProjectId,
    Guid FileId,
    Guid OwnerId) : IRequest<Result>;

public class RestoreProjectFileCommandHandler : IRequestHandler<RestoreProjectFileCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;

    public RestoreProjectFileCommandHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result> Handle(RestoreProjectFileCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result.Failure("PROJECT_NOT_FOUND", "Project not found");

        var file = await _fileRepository.GetByIdForOwnerAsync(request.FileId, request.OwnerId, cancellationToken);
        if (file is null || file.ProjectId != request.ProjectId)
            return Result.Failure("FILE_NOT_FOUND", "File not found");

        if (!file.IsDeleted)
            return Result.Failure("FILE_NOT_DELETED", "File is not deleted");

        // Restore metadata only — set IsDeleted=false, clear DeletedAtUtc
        file.Restore();
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
