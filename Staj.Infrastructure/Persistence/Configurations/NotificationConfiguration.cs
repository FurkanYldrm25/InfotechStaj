// Staj.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.ReferenceType).HasMaxLength(50);
        builder.Property(n => n.CreatedBy).HasMaxLength(100);
        builder.Property(n => n.UpdatedBy).HasMaxLength(100);

        builder.Property(n => n.Type).HasConversion<int>();

        builder.HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.RecipientUserId);
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
        builder.HasIndex(n => n.CreatedAt);

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}