using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Folders;

public record DeleteFolderCommand(
    Guid ProjectId,
    Guid FolderId,
    Guid OwnerId) : IRequest<Result>;

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Result>
{
    private readonly IProjectFolderRepository _folderRepository;

    public DeleteFolderCommandHandler(IProjectFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<Result> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepository.GetByIdForOwnerAsync(request.FolderId, request.OwnerId, cancellationToken);
        if (folder is null || folder.ProjectId != request.ProjectId)
        {
            return Result.Failure("FOLDER_NOT_FOUND", "Folder not found");
        }

        if (!folder.SoftDelete())
        {
            return Result.Failure("FOLDER_SYSTEM_PROTECTED", "System folders cannot be deleted");
        }

        await _folderRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
