namespace Akar.Application.DTOs;

public record OwnerProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
