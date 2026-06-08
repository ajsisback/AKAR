using Akar.Domain.Common;

namespace Akar.Domain.Entities;

/// <summary>
/// Individual owner account for the AKAR platform.
/// Represents a Saudi residential project owner.
/// </summary>
public class Owner : AggregateRoot<Guid>
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? CompanyName { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Owner() { } // EF Core
    private Owner(Guid id) : base(id) { }

    public static Owner Create(
        string fullName,
        string email,
        string phone,
        string passwordHash,
        string? companyName = null)
    {
        var owner = new Owner(Guid.NewGuid())
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            Phone = phone,
            PasswordHash = passwordHash,
            CompanyName = companyName,
            IsActive = true
        };

        return owner;
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

    public void UpdateProfile(string fullName, string? phone)
    {
        FullName = fullName;
        Phone = phone ?? string.Empty;
        SetUpdatedAt();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }
}
