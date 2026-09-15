// Staj.Domain/Entities/Category.cs
using Staj.Domain.Common;

namespace Staj.Domain.Entities;

// İş kategorisi (Web Geliştirme, DevOps, ML vb.)
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<CategorySkill> CategorySkills { get; set; } = new List<CategorySkill>();
}