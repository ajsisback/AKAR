using Akar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Akar.Infrastructure.Persistence;

public class AkarDbContext : DbContext
{
    public AkarDbContext(DbContextOptions<AkarDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AkarDbContext).Assembly);
    }
}
