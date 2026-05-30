using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Domain.Enums;
using Akar.Shared;
using Akar.Shared.Abstractions;
using MediatR;

namespace Akar.Application.Features.Contracts;

/// <summary>
/// Command to generate a PDF for a project contract and save it into the Contracts folder.
/// </summary>
public record GenerateProjectContractPdfCommand(
    Guid ProjectId,
    Guid ContractId,
    Guid OwnerId) : IRequest<Result<GenerateContractPdfResponseDto>>;

public class GenerateProjectContractPdfCommandHandler
    : IRequestHandler<GenerateProjectContractPdfCommand, Result<GenerateContractPdfResponseDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectContractRepository _contractRepository;
    private readonly IProjectFolderRepository _folderRepository;
    private readonly IProjectFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IContractPdfGenerator _pdfGenerator;

    public GenerateProjectContractPdfCommandHandler(
        IProjectRepository projectRepository,
        IProjectContractRepository contractRepository,
        IProjectFolderRepository folderRepository,
        IProjectFileRepository fileRepository,
        IFileStorageService storageService,
        IContractPdfGenerator pdfGenerator)
    {
        _projectRepository = projectRepository;
        _contractRepository = contractRepository;
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _storageService = storageService;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<Result<GenerateContractPdfResponseDto>> Handle(
        GenerateProjectContractPdfCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate project ownership
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, request.OwnerId, cancellationToken);
        if (project is null)
            return Result<GenerateContractPdfResponseDto>.Failure("PROJECT_NOT_FOUND", "Project not found");

        // 2. Validate contract ownership + belongs to project
        var contract = await _contractRepository.GetByIdForOwnerAsync(request.ContractId, request.OwnerId, cancellationToken);
        if (contract is null || contract.ProjectId != request.ProjectId)
            return Result<GenerateContractPdfResponseDto>.Failure("CONTRACT_NOT_FOUND", "Contract not found");

        // 3. Validate contract is not deleted
        if (contract.IsDeleted)
            return Result<GenerateContractPdfResponseDto>.Failure("CONTRACT_NOT_FOUND", "Contract not found");

        // 4. Validate status allows PDF generation (Draft or ReadyForPdf)
        if (contract.Status == ContractStatus.Cancelled)
            return Result<GenerateContractPdfResponseDto>.Failure("CONTRACT_CANCELLED", "Cancelled contracts cannot generate PDF");

        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.ReadyForPdf)
            return Result<GenerateContractPdfResponseDto>.Failure(
                "CONTRACT_NOT_ELIGIBLE_FOR_PDF",
                $"Contract status '{contract.Status}' does not allow PDF generation. Only Draft or ReadyForPdf contracts can generate PDFs.");

        // 5. Find Contracts system folder
        var contractsFolder = await _folderRepository.GetSystemFolderAsync(request.ProjectId, FolderType.Contracts, cancellationToken);
        if (contractsFolder is null)
            return Result<GenerateContractPdfResponseDto>.Failure("CONTRACTS_FOLDER_NOT_FOUND", "Contracts system folder not found");

        // 6. Load owner for PDF content
        // Project.Owner navigation is loaded through the repository include
        // We need Owner entity — get from project context; ProjectRepository doesn't include Owner
        // Use a separate owner lookup via the repository or access via the contract's OwnerId
        // Since we have the contract with template, we need the owner separately
        var owner = project.Owner;
        if (owner is null)
        {
            // Fallback: use minimal owner info from token/claim
            // This shouldn't happen if ProjectRepository includes Owner
            // For now, create a placeholder to not fail
            return Result<GenerateContractPdfResponseDto>.Failure("PDF_GENERATION_FAILED", "Owner data could not be loaded");
        }

        // 7. Generate PDF bytes
        byte[] pdfBytes;
        try
        {
            pdfBytes = _pdfGenerator.Generate(contract, project, owner);
        }
        catch (Exception)
        {
            return Result<GenerateContractPdfResponseDto>.Failure("PDF_GENERATION_FAILED", "Failed to generate contract PDF");
        }

        if (pdfBytes.Length == 0)
            return Result<GenerateContractPdfResponseDto>.Failure("PDF_GENERATION_FAILED", "Generated PDF is empty");

        // 8. Save PDF to physical storage
        var sanitizedTitle = SanitizeFileName(contract.ContractTitle);
        var originalFileName = $"{sanitizedTitle}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        var storedFileName = $"{Guid.NewGuid()}.pdf";
        string storagePath;

        try
        {
            using var pdfStream = new MemoryStream(pdfBytes);
            storagePath = await _storageService.SaveAsync(
                request.OwnerId, request.ProjectId, contractsFolder.Id,
                storedFileName, pdfStream, cancellationToken);
        }
        catch (Exception)
        {
            return Result<GenerateContractPdfResponseDto>.Failure("STORAGE_SAVE_FAILED", "Failed to save PDF file to storage");
        }

        // 9. Create ProjectFile metadata
        ProjectFile pdfFile;
        try
        {
            pdfFile = ProjectFile.Create(
                request.ProjectId,
                request.OwnerId,
                contractsFolder.Id,
                originalFileName,
                storedFileName,
                "application/pdf",
                ".pdf",
                pdfBytes.Length,
                StorageProvider.Local,
                storagePath,
                FileCategory.Document);

            await _fileRepository.AddAsync(pdfFile, cancellationToken);
            await _fileRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result<GenerateContractPdfResponseDto>.Failure("PDF_FILE_METADATA_FAILED", "Failed to create PDF file metadata");
        }

        // 10. Update contract status and PdfFileId
        var marked = contract.MarkPdfGenerated(pdfFile.Id);
        if (!marked)
            return Result<GenerateContractPdfResponseDto>.Failure("CONTRACT_NOT_ELIGIBLE_FOR_PDF", "Failed to update contract status");

        await _contractRepository.SaveChangesAsync(cancellationToken);

        return Result<GenerateContractPdfResponseDto>.Success(
            new GenerateContractPdfResponseDto(
                contract.Id,
                pdfFile.Id,
                contract.Status.ToString(),
                "PDF_GENERATED"));
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
            name = name.Replace(c, '_');
        // Replace spaces with underscores for cleaner filenames
        name = name.Replace(' ', '_');
        return string.IsNullOrWhiteSpace(name) ? "contract" : name;
    }
}
