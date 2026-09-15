// Staj.Infrastructure/Persistence/Configurations/PortfolioItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class PortfolioItemConfiguration : IEntityTypeConfiguration<PortfolioItem>
{
    public void Configure(EntityTypeBuilder<PortfolioItem> builder)
    {
        builder.ToTable("PortfolioItems");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.ProjectUrl).HasMaxLength(500);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.HasOne(p => p.FreelancerProfile)
            .WithMany(fp => fp.PortfolioItems)
            .HasForeignKey(p => p.FreelancerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.FreelancerProfileId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}