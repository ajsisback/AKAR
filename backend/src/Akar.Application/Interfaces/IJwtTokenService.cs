namespace Akar.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid ownerId, string email, string fullName);
}
