// Staj.Application/Common/Interfaces/ICurrentUserService.cs
namespace Staj.Application.Common.Interfaces;

// Aktif kullanıcı bilgilerini almak için servis
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}