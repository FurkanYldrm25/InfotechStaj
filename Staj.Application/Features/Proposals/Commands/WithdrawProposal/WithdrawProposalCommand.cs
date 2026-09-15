// Staj.Application/Features/Proposals/Commands/WithdrawProposal/WithdrawProposalCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Proposals.Commands.WithdrawProposal;

// Gönderilen teklifi geri çekme komutu
public record WithdrawProposalCommand(Guid Id) : IRequest<Result>;