// ============================================================
// Интерфейс: Базовый репозиторий
// Разработчик: Ковалевский И.М. (Backend разработчик)
// ============================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarRent.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}