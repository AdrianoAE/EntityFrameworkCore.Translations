using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Interfaces
{
    public interface IIQueryableAllTranslationsExtensions<TEntity>
        where TEntity : class
    {
        Task<dynamic> FirstAsync(CancellationToken cancellationToken = default);
        Task<dynamic> FirstAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<dynamic> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<dynamic> FirstOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<dynamic> SingleAsync(CancellationToken cancellationToken = default);
        Task<dynamic> SingleAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<dynamic> SingleOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<dynamic> SingleOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<dynamic[]> ToArrayAsync(CancellationToken cancellationToken = default);

        Task<Dictionary<TKey, dynamic>> ToDictionaryAsync<TKey>([NotNull] Func<dynamic, TKey> keySelector, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, dynamic>> ToDictionaryAsync<TKey>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] Func<dynamic, TElement> elementSelector, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] Func<dynamic, TElement> elementSelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);

        Task<List<dynamic>> ToListAsync(CancellationToken cancellationToken = default);
    }
}
