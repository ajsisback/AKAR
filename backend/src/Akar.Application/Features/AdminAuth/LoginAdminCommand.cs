using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.AdminAuth;

public record LoginAdminCommand(
    string Email,
    string Password) : IRequest<Result<AdminAuthResponseDto>>;

public class LoginAdminCommandHandler : IRequestHandler<LoginAdminCommand, Result<AdminAuthResponseDto>>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginAdminCommandHandler(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AdminAuthResponseDto>> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _adminUserRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (admin is null)
        {
            return Result<AdminAuthResponseDto>.Failure("AUTH_INVALID_CREDENTIALS", "Invalid email or password");
        }

        if (!admin.IsActive)
        {
            return Result<AdminAuthResponseDto>.Failure("AUTH_ACCOUNT_INACTIVE", "Account is inactive");
        }

        if (!_passwordHasher.Verify(request.Password, admin.PasswordHash))
        {
            return Result<AdminAuthResponseDto>.Failure("AUTH_INVALID_CREDENTIALS", "Invalid email or password");
        }

        var token = _jwtTokenService.GenerateAdminToken(
            admin.Id, admin.Email, admin.FullName, admin.Role.ToString());

        var adminDto = new AdminDto(
            admin.Id,
            admin.FullName,
            admin.Email,
            admin.Role.ToString());

        return Result<AdminAuthResponseDto>.Success(new AdminAuthResponseDto(token, adminDto));
    }
}
