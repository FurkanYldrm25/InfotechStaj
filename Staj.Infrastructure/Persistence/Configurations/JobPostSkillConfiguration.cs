// Staj.Infrastructure/Persistence/Configurations/JobPostSkillConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staj.Domain.Entities;

namespace Staj.Infrastructure.Persistence.Configurations;

public class JobPostSkillConfiguration : IEntityTypeConfiguration<JobPostSkill>
{
    public void Configure(EntityTypeBuilder<JobPostSkill> builder)
    {
        builder.ToTable("JobPostSkills");
        builder.HasKey(js => new { js.JobPostId, js.SkillId });

        builder.HasOne(js => js.JobPost)
            .WithMany(j => j.JobPostSkills)
            .HasForeignKey(js => js.JobPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(js => js.Skill)
            .WithMany()
            .HasForeignKey(js => js.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(js => js.SkillId);
    }
}