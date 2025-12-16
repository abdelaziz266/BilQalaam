using BilQalaam.Domain.Entities;

namespace BilQalaam.Application.Interfaces
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(ApplicationUser user);
    }
}
