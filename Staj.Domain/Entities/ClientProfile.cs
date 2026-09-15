// Staj.Domain/Entities/ClientProfile.cs
using Staj.Domain.Common;
using Staj.Domain.Enums;

namespace Staj.Domain.Entities;

// İşveren profili
public class ClientProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public ContactPreference ContactPreference { get; set; } = ContactPreference.PlatformMessage;
    public string? ContactPhone { get; set; }
}