using Akar.Application.Interfaces;
using Akar.Domain.Entities;
using Akar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly AkarDbContext _context;

    public OwnerRepository(AkarDbContext context) => _context = context;

    public async Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Owners.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Owners.FirstOrDefaultAsync(o => o.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Owners.AnyAsync(o => o.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task AddAsync(Owner owner, CancellationToken cancellationToken = default)
        => await _context.Owners.AddAsync(owner, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
