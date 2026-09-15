// Staj.Application/Features/Clients/Queries/GetClientById/GetClientByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Interfaces;
using Staj.Application.Common.Models;
using Staj.Application.Features.Clients.Dtos;

namespace Staj.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQueryHandler
    : IRequestHandler<GetClientByIdQuery, Result<ClientProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClientProfileDto>> Handle(
        GetClientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.ClientProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile is null)
            return Result<ClientProfileDto>.Fail("Client bulunamadı.");

        var dto = new ClientProfileDto(
            profile.Id, profile.UserId,
            profile.User.FirstName, profile.User.LastName, profile.User.Email ?? string.Empty,
            profile.User.ProfileImageUrl,
            profile.CompanyName, profile.Industry, profile.About, profile.WebsiteUrl,
            profile.Country, profile.City,
            profile.ContactPreference, profile.ContactPhone);

        return Result<ClientProfileDto>.Ok(dto);
    }
}