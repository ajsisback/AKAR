using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using Akar.Shared.Abstractions;
using MediatR;

namespace Akar.Application.Features.Contracts;

/// <summary>
/// Command to upload a signed contract PDF file, link it to the contract,
/// and update status to SignedUploaded.
/// </summary>
public record UploadSignedContractFileCommand(
    Guid ProjectId,
    Guid ContractId,
    Guid OwnerId,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream) : IRequest<Result<UploadSignedContractFileResponseDto>>;

public class UploadSignedContractFileCommandHandler
    : IRequestHandler<UploadSignedContractFileCommand, Result<UploadSignedContractFileResponseDto>>
{
    private const long MaxSignedFileSize = 20 * 1024 * 1024; // 20 MB
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf"
    };
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectContractRepository _contractRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IProjectTimelineEventWriter _timelineWriter;

    public UploadSignedContractFileCommandHandler(
        IProjectRepository projectRepository,
        IProjectContractRepository contractRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository,
        IFileStorageService storageService,
        IProjectTimelineEventWriter timelineWriter)
    {
        _projectRepository = projectRepository;
        _contractRepository = contractRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
        _timelineWriter = timelineWriter;
    }

    public async Task<Result<UploadSignedContractFileResponseDto>> Handle(
        UploadSignedContractFileCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate file is present
        if (request.FileStream is null || request.FileSizeBytes == 0)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "SIGNED_FILE_REQUIRED", "A signed contract PDF file is required");

        // 2. Validate file extension
        var extension = Path.GetExtension(request.OriginalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "SIGNED_FILE_MUST_BE_PDF", "Signed contract file must be a PDF");

        // 3. Validate content type
        if (!AllowedContentTypes.Contains(request.ContentType))
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "SIGNED_FILE_MUST_BE_PDF", "Signed contract file must be a PDF (invalid content type)");

        // 4. Validate file size
        if (request.FileSizeBytes > MaxSignedFileSize)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "SIGNED_FILE_TOO_LARGE", $"Signed contract file must be 20 MB or less (received {request.FileSizeBytes / (1024 * 1024)} MB)");

        // 5. Validate project ownership
        var project = await _projectRepository.GetByIdForOwnerAsync(
            request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "PROJECT_NOT_FOUND", "Project not found");

        // 6. Validate contract ownership + belongs to project
        var contract = await _contractRepository.GetByIdForOwnerAsync(
            request.ContractId, request.OwnerId, cancellationToken);
        if (contract is null || contract.ProjectId != request.ProjectId)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_NOT_FOUND", "Contract not found");

        // 7. Validate contract is not deleted
        if (contract.IsDeleted)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_NOT_FOUND", "Contract not found");

        // 8. Validate contract status allows signed upload
        if (contract.Status == ContractStatus.Cancelled)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_CANCELLED", "Cancelled contracts cannot accept signed files");

        if (contract.Status != ContractStatus.PdfGenerated)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_NOT_PDF_GENERATED",
                $"Contract must have a generated PDF before uploading signed version. Current status: {contract.Status}");

        // 9. Validate contract has PdfFileId
        if (contract.PdfFileId is null)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_NOT_PDF_GENERATED", "Contract does not have a generated PDF");

        // 10. Validate contract is not already signed
        if (contract.SignedFileId is not null)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_ALREADY_SIGNED", "Contract already has a signed file uploaded");

        // 11. Find Contracts system folder
        var contractsFolder = await _folderRepository.GetSystemFolderAsync(
            request.ProjectId, FolderType.Contracts, cancellationToken);
        if (contractsFolder is null)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACTS_FOLDER_NOT_FOUND", "Contracts system folder not found");

        // 12. Save file to physical storage
        var storedFileName = $"{Guid.NewGuid()}.pdf";
        string storagePath;

        try
        {
            storagePath = await _storageService.SaveAsync(
                request.OwnerId, request.ProjectId, contractsFolder.Id,
                storedFileName, request.FileStream, cancellationToken);
        }
        catch (Exception)
        {
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "STORAGE_SAVE_FAILED", "Failed to save signed contract file to storage");
        }

        // 13. Create ProjectFile metadata
        ProjectFile signedFile;
        try
        {
            signedFile = ProjectFile.Create(
                request.ProjectId,
                request.OwnerId,
                contractsFolder.Id,
                request.OriginalFileName,
                storedFileName,
                "application/pdf",
                ".pdf",
                request.FileSizeBytes,
                StorageProvider.Local,
                storagePath,
                FileCategory.Document);

            await _fileRepository.AddAsync(signedFile, cancellationToken);
            await _fileRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "SIGNED_FILE_METADATA_FAILED", "Failed to create signed file metadata");
        }

        // 14. Update contract: link signed file and update status
        var marked = contract.MarkSignedUploaded(signedFile.Id);
        if (!marked)
            return Result<UploadSignedContractFileResponseDto>.Failure(
                "CONTRACT_ALREADY_SIGNED", "Failed to update contract status");

        await _contractRepository.SaveChangesAsync(cancellationToken);

        // 15. Create timeline event
        await _timelineWriter.AddSystemEventAsync(
            project.Id, project.OwnerId, project.CurrentStage,
            TimelineEventType.ContractSignedUploaded, TimelineSourceType.ProjectContract, contract.Id,
            "Signed contract uploaded", $"Signed PDF uploaded for: {contract.ContractTitle}",
            cancellationToken);

        return Result<UploadSignedContractFileResponseDto>.Success(
            new UploadSignedContractFileResponseDto(
                contract.Id,
                signedFile.Id,
                contract.Status.ToString(),
                "SIGNED_CONTRACT_UPLOADED"));
    }
}
