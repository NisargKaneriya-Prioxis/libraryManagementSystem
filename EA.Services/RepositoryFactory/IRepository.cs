using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace EA.Services.RepositoryFactory;

public interface IRepository<T> where T : class
{
    Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        bool enableTracking = true,
        bool ignoreQueryFilters = false);

    Task<IList<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        bool enableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        bool enableTracking = true,
        bool ignoreQueryFilters = false);

    ValueTask<EntityEntry<T>> InsertAsync(T entity,
        CancellationToken cancellationToken = default);

    Task InsertAsync(params T[] entities);

    Task InsertAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default);


    //Task Update(T entity);
    //Task Update(params T[] entities);
    //Task Update(IEnumerable<T> entities);

    EntityEntry<T> Update(T entity);
    void Update(T[] entities);
    void Update(IEnumerable<T> entities);

    void Delete(T entity);

    void Delete(params T[] entities);

    void Delete(IEnumerable<T> entities);

    Task InsertRangeAsync(IEnumerable<T> entities);
}