using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Files;

public record DeleteProjectFileCommand(
    Guid ProjectId,
    Guid FileId,
    Guid OwnerId) : IRequest<Result>;

public class DeleteProjectFileCommandHandler : IRequestHandler<DeleteProjectFileCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _fileRepository;

    public DeleteProjectFileCommandHandler(
        IProjectRepository projectRepository,
        IProjectFileRepository fileRepository)
    {
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
    }

    public async Task<Result> Handle(DeleteProjectFileCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result.Failure("PROJECT_NOT_FOUND", "Project not found");

        var file = await _fileRepository.GetByIdForOwnerAsync(request.FileId, request.OwnerId, cancellationToken);
        if (file is null || file.ProjectId != request.ProjectId)
            return Result.Failure("FILE_NOT_FOUND", "File not found");

        if (file.IsDeleted)
            return Result.Failure("FILE_ALREADY_DELETED", "File is already deleted");

        // Soft-delete only — do not physically delete file in Sprint 2B
        file.SoftDelete();
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
