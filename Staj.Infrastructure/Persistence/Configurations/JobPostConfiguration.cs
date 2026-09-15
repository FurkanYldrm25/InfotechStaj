// Staj.Infrastructure/Persistence/Configurations/JobPostConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class JobPostConfiguration : IEntityTypeConfiguration<JobPost>
{
    public void Configure(EntityTypeBuilder<JobPost> builder)
    {
        builder.ToTable("JobPosts");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).HasMaxLength(200).IsRequired();
        builder.Property(j => j.Slug).HasMaxLength(220).IsRequired();
        builder.Property(j => j.Description).HasMaxLength(5000).IsRequired();
        builder.Property(j => j.Currency).HasMaxLength(10);
        builder.Property(j => j.Country).HasMaxLength(80);
        builder.Property(j => j.City).HasMaxLength(80);
        builder.Property(j => j.CreatedBy).HasMaxLength(100);
        builder.Property(j => j.UpdatedBy).HasMaxLength(100);

        builder.Property(j => j.BudgetMin).HasColumnType("decimal(12,2)");
        builder.Property(j => j.BudgetMax).HasColumnType("decimal(12,2)");

        builder.Property(j => j.WorkMode).HasConversion<int>();
        builder.Property(j => j.Status).HasConversion<int>();

        builder.HasOne(j => j.ClientProfile)
            .WithMany()
            .HasForeignKey(j => j.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Category)
            .WithMany()
            .HasForeignKey(j => j.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(j => j.Slug).IsUnique();
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.CategoryId);
        builder.HasIndex(j => j.ClientProfileId);
        builder.HasIndex(j => j.PublishedAt);

        builder.HasQueryFilter(j => !j.IsDeleted);
    }
}