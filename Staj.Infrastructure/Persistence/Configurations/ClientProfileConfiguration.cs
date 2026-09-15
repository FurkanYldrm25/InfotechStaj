// Staj.Infrastructure/Persistence/Configurations/ClientProfileConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    public void Configure(EntityTypeBuilder<ClientProfile> builder)
    {
        builder.ToTable("ClientProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompanyName).HasMaxLength(150);
        builder.Property(p => p.Industry).HasMaxLength(100);
        builder.Property(p => p.About).HasMaxLength(2000);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500);
        builder.Property(p => p.Country).HasMaxLength(80);
        builder.Property(p => p.City).HasMaxLength(80);
        builder.Property(p => p.ContactPhone).HasMaxLength(30);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.Property(p => p.ContactPreference).HasConversion<int>();

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<ClientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}