using Akar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Akar.Infrastructure.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("owners");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(o => o.Email)
            .IsUnique();

        builder.Property(o => o.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(o => o.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(200);

        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(o => o.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(o => o.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Navigation
        builder.HasMany<Project>()
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
