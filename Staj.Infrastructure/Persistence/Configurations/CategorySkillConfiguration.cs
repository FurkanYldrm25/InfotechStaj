// Staj.Infrastructure/Persistence/Configurations/CategorySkillConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

// Category-Skill join tablosu EF konfigürasyonu
public class CategorySkillConfiguration : IEntityTypeConfiguration<CategorySkill>
{
    public void Configure(EntityTypeBuilder<CategorySkill> builder)
    {
        builder.ToTable("CategorySkills");

        builder.HasKey(cs => new { cs.CategoryId, cs.SkillId });

        builder.HasOne(cs => cs.Category)
            .WithMany(c => c.CategorySkills)
            .HasForeignKey(cs => cs.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Skill)
            .WithMany(s => s.CategorySkills)
            .HasForeignKey(cs => cs.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cs => cs.SkillId);
    }
}