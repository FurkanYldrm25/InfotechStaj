// Staj.Application/Features/Proposals/Commands/AcceptProposal/AcceptProposalCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Proposals.Commands.AcceptProposal;

// Teklifi kabul etme komutu
public record AcceptProposalCommand(
    Guid Id,
    string? ResponseNote
) : IRequest<Result>;