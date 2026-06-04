namespace Akar.Application.DTOs;

public record UpdateOwnerProfileRequest(
    string FullName,
    string? Phone);
