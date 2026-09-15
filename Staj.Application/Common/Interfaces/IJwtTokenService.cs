// Staj.Application/Common/Interfaces/IJwtTokenService.cs
using Staj.Domain.Entities;

namespace Staj.Application.Common.Interfaces;

// JWT token üretimi için servis sözleşmesi
public interface IJwtTokenService
{
    Task<TokenResult> GenerateTokensAsync(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}

// Token üretim sonucunu taşıyan kayıt
public record TokenResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);