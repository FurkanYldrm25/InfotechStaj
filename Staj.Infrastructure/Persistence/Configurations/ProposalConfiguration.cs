// Staj.Infrastructure/Persistence/Configurations/ProposalConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CoverMessage).HasMaxLength(3000).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(10);
        builder.Property(p => p.ResponseNote).HasMaxLength(1000);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.Property(p => p.ProposedRate).HasColumnType("decimal(12,2)");

        builder.Property(p => p.Kind).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();

        builder.HasOne(p => p.JobPost)
            .WithMany()
            .HasForeignKey(p => p.JobPostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FreelancerProfile)
            .WithMany()
            .HasForeignKey(p => p.FreelancerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ClientProfile)
            .WithMany()
            .HasForeignKey(p => p.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.JobPostId);
        builder.HasIndex(p => p.FreelancerProfileId);
        builder.HasIndex(p => p.ClientProfileId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => new { p.Kind, p.Status });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}