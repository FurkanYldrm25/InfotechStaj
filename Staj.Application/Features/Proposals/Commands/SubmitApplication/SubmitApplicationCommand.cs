// Staj.Application/Features/Proposals/Commands/SubmitApplication/SubmitApplicationCommand.cs
using MediatR;
using Staj.Application.Common.Models;

namespace Staj.Application.Features.Proposals.Commands.SubmitApplication;

// Freelancer'ın bir ilana başvurusu
public record SubmitApplicationCommand(
    Guid JobPostId,
    string CoverMessage,
    decimal? ProposedRate,
    int? ProposedDurationDays,
    string? Currency
) : IRequest<Result<Guid>>;