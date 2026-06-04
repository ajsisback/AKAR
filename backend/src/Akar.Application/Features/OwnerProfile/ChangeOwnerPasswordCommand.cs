using Akar.Application.Interfaces;
using MediatR;

namespace Akar.Application.Features.OwnerProfile;

public record ChangeOwnerPasswordCommand(Guid OwnerId, string CurrentPassword, string NewPassword, string ConfirmNewPassword) : IRequest;

public class ChangeOwnerPasswordCommandHandler : IRequestHandler<ChangeOwnerPasswordCommand>
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeOwnerPasswordCommandHandler(
        IOwnerRepository ownerRepository, 
        IPasswordHasher passwordHasher)
    {
        _ownerRepository = ownerRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangeOwnerPasswordCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            throw new InvalidOperationException("OWNER_NOT_FOUND");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, owner.PasswordHash))
        {
            throw new InvalidOperationException("CURRENT_PASSWORD_INVALID");
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            throw new InvalidOperationException("PASSWORD_SAME_AS_CURRENT");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidOperationException("PASSWORD_CONFIRMATION_MISMATCH");
        }

        // Validate password strength (handler-level guard because ValidationBehavior
        // only runs for IRequest<TResponse>, not bare IRequest)
        if (string.IsNullOrEmpty(request.NewPassword)
            || request.NewPassword.Length < 8
            || !request.NewPassword.Any(char.IsUpper)
            || !request.NewPassword.Any(char.IsLower)
            || !request.NewPassword.Any(char.IsDigit)
            || request.NewPassword.All(char.IsLetterOrDigit))
        {
            throw new InvalidOperationException("PASSWORD_TOO_WEAK");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        owner.ChangePassword(newHash);

        await _ownerRepository.SaveChangesAsync(cancellationToken);
    }
}
