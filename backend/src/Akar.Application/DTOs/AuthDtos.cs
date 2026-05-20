namespace Akar.Application.DTOs;

public record AuthResponseDto(
    string Token,
    OwnerDto Owner);

public record LoginRequest(
    string Email,
    string Password);

public record RegisterRequest(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string? CompanyName);
