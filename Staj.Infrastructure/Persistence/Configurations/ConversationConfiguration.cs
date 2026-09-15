// Staj.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.LastMessagePreview).HasMaxLength(200);
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        builder.HasOne(c => c.Participant1)
            .WithMany()
            .HasForeignKey(c => c.Participant1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Participant2)
            .WithMany()
            .HasForeignKey(c => c.Participant2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı çift için tek conversation garantisi
        builder.HasIndex(c => new { c.Participant1Id, c.Participant2Id }).IsUnique();
        builder.HasIndex(c => c.LastMessageAt);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}