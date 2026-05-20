namespace Akar.Application.DTOs;

public record OwnerDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string? CompanyName,
    bool IsActive,
    DateTime CreatedAtUtc);
