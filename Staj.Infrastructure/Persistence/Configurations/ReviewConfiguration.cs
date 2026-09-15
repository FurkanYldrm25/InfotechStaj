// Staj.Infrastructure/Persistence/Configurations/ReviewConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.IsVisible).HasDefaultValue(true);

        builder.HasOne(r => r.Proposal)
            .WithMany()
            .HasForeignKey(r => r.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Target)
            .WithMany()
            .HasForeignKey(r => r.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.TargetUserId);
        builder.HasIndex(r => r.AuthorUserId);
        builder.HasIndex(r => r.ProposalId);

        // Aynı Proposal + Author kombinasyonu tek yorum yazabilir
        builder.HasIndex(r => new { r.ProposalId, r.AuthorUserId }).IsUnique();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}