// Staj.Infrastructure/Persistence/Configurations/FreelancerProfileConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class FreelancerProfileConfiguration : IEntityTypeConfiguration<FreelancerProfile>
{
    public void Configure(EntityTypeBuilder<FreelancerProfile> builder)
    {
        builder.ToTable("FreelancerProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(150);
        builder.Property(p => p.Bio).HasMaxLength(2000);
        builder.Property(p => p.Currency).HasMaxLength(10);
        builder.Property(p => p.Country).HasMaxLength(80);
        builder.Property(p => p.City).HasMaxLength(80);
        builder.Property(p => p.CvUrl).HasMaxLength(500);
        builder.Property(p => p.LinkedInUrl).HasMaxLength(500);
        builder.Property(p => p.GitHubUrl).HasMaxLength(500);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.Property(p => p.HourlyRateMin).HasColumnType("decimal(10,2)");
        builder.Property(p => p.HourlyRateMax).HasColumnType("decimal(10,2)");

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<FreelancerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.IsAvailable);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}