// Staj.Domain/Enums/ProposalKind.cs
namespace Staj.Domain.Enums;

// Teklifin türü
public enum ProposalKind
{
    // Freelancer'ın bir ilana başvurusu
    Application = 1,

    // Client'ın freelancer'a doğrudan teklifi (ilansız)
    DirectOffer = 2
}