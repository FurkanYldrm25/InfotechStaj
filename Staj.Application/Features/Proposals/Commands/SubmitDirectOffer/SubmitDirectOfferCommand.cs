// Staj.Application/Features/Proposals/Commands/SubmitDirectOffer/SubmitDirectOfferCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Proposals.Commands.SubmitDirectOffer;

// Client'ın Freelancer'a doğrudan teklifi (ilansız)
public record SubmitDirectOfferCommand(
    Guid FreelancerProfileId,
    string CoverMessage,
    decimal? ProposedRate,
    int? ProposedDurationDays,
    string? Currency
) : IRequest<Result<Guid>>;