using Akar.Domain.Common;
using Akar.Domain.Enums;

namespace Akar.Domain.Entities;

/// <summary>
/// Admin user account for the AKAR operating company.
/// Admins access the Angular Web Admin Portal to manage and monitor the system.
/// AdminUser is completely separate from Owner — they are different user contexts.
/// </summary>
public class AdminUser : AggregateRoot<Guid>
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AdminRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AdminUser() { } // EF Core
    private AdminUser(Guid id) : base(id) { }

    public static AdminUser Create(
        string fullName,
        string email,
        string passwordHash,
        AdminRole role)
    {
        var admin = new AdminUser(Guid.NewGuid())
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };

        return admin;
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }
}
