namespace Akar.Application.DTOs;

/// <summary>
/// Trash listing for a project — contains deleted files and deleted custom folders.
/// System folders are never included because they should never be deleted.
/// </summary>
public record ProjectTrashDto(
    List<ProjectFileDto> DeletedFiles,
    List<ProjectFolderDto> DeletedFolders);
