using Akar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Akar.Infrastructure.Persistence;

public class AkarDbContext : DbContext
{
    public AkarDbContext(DbContextOptions<AkarDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectFolder> ProjectFolders => Set<ProjectFolder>();
    public DbSet<ProjectFile> ProjectFiles => Set<ProjectFile>();
    public DbSet<ProjectFollower> ProjectFollowers => Set<ProjectFollower>();
    public DbSet<FollowerUploadLink> FollowerUploadLinks => Set<FollowerUploadLink>();
    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<ProjectContract> ProjectContracts => Set<ProjectContract>();
    public DbSet<ProjectTimelineEvent> ProjectTimelineEvents => Set<ProjectTimelineEvent>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AkarDbContext).Assembly);
    }
}
