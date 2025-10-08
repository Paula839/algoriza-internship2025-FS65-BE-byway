using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Byway.Infrastructure.Data;
using Byway.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Byway.Core.Repository
{
    public class InstructorRepository : Repository<Instructor>, IInstructorRepository
    {
        public InstructorRepository(BywayDbContext db) : base(db)
        {
        }

        public override async Task<List<Instructor>> GetAllAsync() => await _db.Instructors
                .Include(i => i.Courses)
                .AsNoTracking()
                .ToListAsync();

        public override async Task<Instructor?> GetByIdAsync(int id) => await _db.Instructors
                .Include(i => i.Courses)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

        public override async Task<List<Instructor>> GetAllByIdAsync(HashSet<int> ids) => await _db.Instructors
                .Where(e => ids.Contains(EF.Property<int>(e, "Id")))
                .ToListAsync();
    }
}
