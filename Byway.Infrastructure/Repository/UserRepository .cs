using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Byway.Infrastructure.Data;
using Byway.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Byway.Core.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(BywayDbContext db) : base(db)
        {
        }

        public override async Task<List<User>> GetAllAsync() => await _db.Users
                .Include(u => u.Courses)
                .ToListAsync();

        public override async Task<User?> GetByIdAsync(int id) => await _db.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == id);

        public override async Task<List<User>> GetAllByIdAsync(HashSet<int> ids) => await _db.Users
                .Where(e => ids.Contains(EF.Property<int>(e, "Id")))
                .Include(u => u.Courses)    
                .ToListAsync();

        public async Task<bool> ExistsByEmailAsync(string email) => await _db.Users
                .AnyAsync(u => u.Email == email.Trim().ToLower());

        public async Task<bool> ExistsByUsernameAsync(string username) => await _db.Users
                .AnyAsync(u => u.Username == username.Trim().ToLower());

        public async Task<User?> GetByEmailAsync(string email) => await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());

        public async Task<User?> GetByUsernameAsync(string username) => await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username.Trim().ToLower());

    }
}
