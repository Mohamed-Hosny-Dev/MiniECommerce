using System.Linq.Expressions;

namespace MiniECommerce.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            string[]? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            string[]? includes = null);

        /// <summary>
        /// Database-side paging (COUNT + OFFSET/FETCH). The requested page is clamped
        /// into the valid range, and the page actually returned is reported back.
        /// Read-only (no change tracking).
        /// </summary>
        Task<PagedResult<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string[]? includes = null);

        /// <summary>Read-only projection executed in the database.</summary>
        Task<IReadOnlyList<TResult>> SelectAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
