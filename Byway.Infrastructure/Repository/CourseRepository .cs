using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Byway.Infrastructure.Data;
using Byway.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Byway.Core.Repository
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(BywayDbContext db) : base(db) { }

        public override async Task<List<Course>> GetAllAsync() => await _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Users)
                .AsNoTracking()
                .ToListAsync();

        public override IQueryable<Course> Query() => _db.Set<Course>()
            .Include(c => c.Instructor)
            .AsQueryable();
        

        public override async Task<Course?> GetByIdAsync(int id) => await _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Users)
                .Include(c => c.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

        public override async Task<List<Course>> GetAllByIdAsync(HashSet<int> ids) => await _db.Courses
                .Where(e => ids.Contains(EF.Property<int>(e, "Id")))
                .ToListAsync();

    }
}
