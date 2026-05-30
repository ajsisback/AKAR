using Akar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class FollowerUploadLinkConfiguration : IEntityTypeConfiguration<FollowerUploadLink>
{
    public void Configure(EntityTypeBuilder<FollowerUploadLink> builder)
    {
        builder.ToTable("follower_upload_links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(l => l.OwnerId).HasColumnName("owner_id").IsRequired();
        builder.Property(l => l.FollowerId).HasColumnName("follower_id").IsRequired();

        builder.Property(l => l.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(l => l.TokenPreview)
            .HasColumnName("token_preview")
            .HasMaxLength(20);

        builder.Property(l => l.ExpiresAtUtc).HasColumnName("expires_at_utc");

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(l => l.IsRevoked)
            .HasColumnName("is_revoked")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(l => l.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(l => l.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(l => l.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.Property(l => l.LastUsedAtUtc).HasColumnName("last_used_at_utc");

        // Indexes
        builder.HasIndex(l => l.ProjectId);
        builder.HasIndex(l => l.OwnerId);
        builder.HasIndex(l => l.FollowerId);
        builder.HasIndex(l => l.TokenHash).IsUnique();
        builder.HasIndex(l => l.IsRevoked);
        builder.HasIndex(l => l.ExpiresAtUtc);

        // Relationships
        builder.HasOne(l => l.Project).WithMany().HasForeignKey(l => l.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Owner).WithMany().HasForeignKey(l => l.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Follower).WithMany().HasForeignKey(l => l.FollowerId).OnDelete(DeleteBehavior.Restrict);
    }
}
