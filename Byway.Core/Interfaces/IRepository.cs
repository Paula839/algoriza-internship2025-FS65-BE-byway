using Byway.Core.Entities;

namespace Byway.Core.Interfaces
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllByIdAsync(HashSet<int> ids);
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T course);
        Task UpdateAsync(T course);
        Task SaveAsync();
        Task DeleteAsync(T entity);
        IQueryable<T> Query();
    }
}
