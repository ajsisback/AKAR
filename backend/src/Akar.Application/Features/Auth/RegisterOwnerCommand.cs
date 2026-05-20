using Akar.Application.DTOs;
using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.Auth;

public record RegisterOwnerCommand(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string? CompanyName) : IRequest<Result<AuthResponseDto>>;

public class RegisterOwnerCommandHandler : IRequestHandler<RegisterOwnerCommand, Result<AuthResponseDto>>
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterOwnerCommandHandler(
        IOwnerRepository ownerRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _ownerRepository = ownerRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterOwnerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _ownerRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result<AuthResponseDto>.Failure("AUTH_EMAIL_EXISTS", "Email already registered");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var owner = Owner.Create(
            request.FullName,
            request.Email,
            request.Phone,
            passwordHash,
            request.CompanyName);

        await _ownerRepository.AddAsync(owner, cancellationToken);
        await _ownerRepository.SaveChangesAsync(cancellationToken);

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
