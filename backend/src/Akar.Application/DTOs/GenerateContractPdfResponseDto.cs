namespace Akar.Application.DTOs;

public record GenerateContractPdfResponseDto(
    Guid ContractId,
    Guid PdfFileId,
    string Status,
    string Message);
