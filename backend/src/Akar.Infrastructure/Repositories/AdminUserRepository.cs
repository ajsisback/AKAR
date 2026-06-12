using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AkarDbContext _context;

    public AdminUserRepository(AkarDbContext context) => _context = context;

    public async Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.AnyAsync(a => a.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(AdminUser admin, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.AddAsync(admin, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
