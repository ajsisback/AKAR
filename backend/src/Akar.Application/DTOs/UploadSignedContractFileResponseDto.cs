namespace Akar.Application.DTOs;

public record UploadSignedContractFileResponseDto(
    Guid ContractId,
    Guid SignedFileId,
    string Status,
    string Message);
