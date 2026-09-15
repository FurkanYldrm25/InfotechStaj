// Staj.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Staj.Application.Common.Models;
using Staj.Domain.Entities;

namespace Staj.Application.Features.Auth.Commands.Register;

// Kayıt komutu işleyicisi
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result<RegisterResponse>.Fail("Bu email zaten kayıtlı.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return Result<RegisterResponse>.Fail("Kullanıcı oluşturulamadı.", errors);
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = request.Role });
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return Result<RegisterResponse>.Ok(
            new RegisterResponse(user.Id, user.Email!, request.Role),
            "Kayıt başarılı.");
    }
}