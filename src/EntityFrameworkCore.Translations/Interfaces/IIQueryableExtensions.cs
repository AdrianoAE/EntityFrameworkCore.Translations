using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Interfaces
{
    public interface IIQueryableTranslationExtensions<TEntity>
        where TEntity : class
    {
        Task<TEntity> FirstAsync(CancellationToken cancellationToken = default);
        Task<TEntity> FirstAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<TEntity> FirstOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity> SingleAsync(CancellationToken cancellationToken = default);
        Task<TEntity> SingleAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity> SingleOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<TEntity> SingleOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity[]> ToArrayAsync(CancellationToken cancellationToken = default);

        Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>([NotNull] Func<TEntity, TKey> keySelector, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] Func<TEntity, TElement> elementSelector, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] Func<TEntity, TElement> elementSelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);

        Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);
    }
}
