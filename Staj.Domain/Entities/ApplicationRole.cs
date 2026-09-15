// Staj.Domain/Entities/ApplicationRole.cs
using Microsoft.AspNetCore.Identity;

namespace Staj.Domain.Entities;

// ASP.NET Core Identity rol sınıfı
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}