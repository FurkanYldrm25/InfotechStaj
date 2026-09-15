// Staj.Application/Features/Proposals/Commands/RejectProposal/RejectProposalCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Proposals.Commands.RejectProposal;

// Teklifi reddetme komutu
public record RejectProposalCommand(
    Guid Id,
    string? ResponseNote
) : IRequest<Result>;