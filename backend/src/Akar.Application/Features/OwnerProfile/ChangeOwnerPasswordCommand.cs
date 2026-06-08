using Akar.Application.Interfaces;
using Akar.Shared;
using MediatR;

namespace Akar.Application.Features.OwnerProfile;

public record ChangeOwnerPasswordCommand(Guid OwnerId, string CurrentPassword, string NewPassword, string ConfirmNewPassword) : IRequest<Result>;

public class ChangeOwnerPasswordCommandHandler : IRequestHandler<ChangeOwnerPasswordCommand, Result>
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

    public async Task<Result> Handle(ChangeOwnerPasswordCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            return Result.Failure("OWNER_NOT_FOUND", "Owner not found");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, owner.PasswordHash))
        {
            return Result.Failure("CURRENT_PASSWORD_INVALID", "Current password is incorrect");
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            return Result.Failure("PASSWORD_SAME_AS_CURRENT", "New password cannot be the same as current password");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Result.Failure("PASSWORD_CONFIRMATION_MISMATCH", "Password confirmation does not match");
        }

        // Defense-in-depth: handler-level password strength guard.
        // FluentValidation now runs via the pipeline (ChangeOwnerPasswordCommandValidator),
        // but this remains as a safety net.
        if (string.IsNullOrEmpty(request.NewPassword)
            || request.NewPassword.Length < 8
            || !request.NewPassword.Any(char.IsUpper)
            || !request.NewPassword.Any(char.IsLower)
            || !request.NewPassword.Any(char.IsDigit)
            || request.NewPassword.All(char.IsLetterOrDigit))
        {
            return Result.Failure("PASSWORD_TOO_WEAK", "Password does not meet strength requirements");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        owner.ChangePassword(newHash);

        await _ownerRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
