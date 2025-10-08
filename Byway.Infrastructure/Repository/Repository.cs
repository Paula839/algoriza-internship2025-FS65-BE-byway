using Byway.Core.Interfaces;
using Byway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Byway.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        protected readonly BywayDbContext _db;

        public Repository(BywayDbContext db)
        {
            _db = db;
        }

        public virtual async Task<List<T>> GetAllAsync() => await _db.Set<T>().ToListAsync();
        
        public virtual IQueryable<T> Query()
        {
            return _db.Set<T>().AsQueryable();
        }

        public virtual async Task<T?> GetByIdAsync(int id) => await _db.Set<T>().FindAsync(id);
        public virtual async Task<List<T>> GetAllByIdAsync(HashSet<int> ids) => await _db.Set<T>().Where(e => ids.Contains(EF.Property<int>(e, "Id"))).ToListAsync();


        public async Task AddAsync(T entity)
        {
            _db.Set<T>().Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Set<T>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {

            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync();
            
        }

  
    }
}
