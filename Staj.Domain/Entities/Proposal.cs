// Staj.Domain/Entities/Proposal.cs
using Staj.Domain.Common;
using Staj.Domain.Enums;

namespace Staj.Domain.Entities;

// Başvuru veya doğrudan teklif
public class Proposal : BaseEntity
{
    // Application ise dolu, DirectOffer ise null
    public Guid? JobPostId { get; set; }
    public JobPost? JobPost { get; set; }

    public Guid FreelancerProfileId { get; set; }
    public FreelancerProfile FreelancerProfile { get; set; } = null!;

    public Guid ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; } = null!;

    public ProposalKind Kind { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    public string CoverMessage { get; set; } = string.Empty;
    public decimal? ProposedRate { get; set; }
    public int? ProposedDurationDays { get; set; }
    public string? Currency { get; set; }

    // Yanıt bilgileri
    public DateTime? RespondedAt { get; set; }
    public string? ResponseNote { get; set; }
}