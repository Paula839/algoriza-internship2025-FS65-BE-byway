using Byway.Core.Entities;

namespace Byway.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
