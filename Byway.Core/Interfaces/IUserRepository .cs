using Byway.Core.Entities;

namespace Byway.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);

        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string email);
    }
}
