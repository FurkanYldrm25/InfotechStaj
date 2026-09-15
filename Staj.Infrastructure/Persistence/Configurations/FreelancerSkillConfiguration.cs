// Staj.Infrastructure/Persistence/Configurations/FreelancerSkillConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class FreelancerSkillConfiguration : IEntityTypeConfiguration<FreelancerSkill>
{
    public void Configure(EntityTypeBuilder<FreelancerSkill> builder)
    {
        builder.ToTable("FreelancerSkills");
        builder.HasKey(fs => new { fs.FreelancerProfileId, fs.SkillId });

        builder.HasOne(fs => fs.FreelancerProfile)
            .WithMany(p => p.FreelancerSkills)
            .HasForeignKey(fs => fs.FreelancerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fs => fs.Skill)
            .WithMany()
            .HasForeignKey(fs => fs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fs => fs.SkillId);
    }
}