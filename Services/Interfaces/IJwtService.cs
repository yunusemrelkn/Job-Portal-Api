using JobPortal.Api.Models.Entities;
using System.Security.Claims;

namespace JobPortal.Api.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
    }
}
