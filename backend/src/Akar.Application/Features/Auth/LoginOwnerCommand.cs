using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Auth;

public record LoginOwnerCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponseDto>>;

public class LoginOwnerCommandHandler : IRequestHandler<LoginOwnerCommand, Result<AuthResponseDto>>
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginOwnerCommandHandler(
        IOwnerRepository ownerRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _ownerRepository = ownerRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (owner is null)
        {
            return Result<AuthResponseDto>.Failure("AUTH_INVALID_CREDENTIALS", "Invalid email or password");
        }

        if (!owner.IsActive)
        {
            return Result<AuthResponseDto>.Failure("AUTH_ACCOUNT_INACTIVE", "Account is inactive");
        }

        if (!_passwordHasher.Verify(request.Password, owner.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("AUTH_INVALID_CREDENTIALS", "Invalid email or password");
        }

        var token = _jwtTokenService.GenerateToken(owner.Id, owner.Email, owner.FullName);

        var ownerDto = new OwnerDto(
            owner.Id,
            owner.FullName,
            owner.Email,
            owner.Phone,
            owner.CompanyName,
            owner.IsActive,
            owner.CreatedAtUtc);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(token, ownerDto));
    }
}
