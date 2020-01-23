using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public sealed class AllTranslationsQuery<TEntity> : IIQueryableAllTranslationsExtensions<TEntity>
        where TEntity : class
    {
        private IQueryable<TEntity> _query;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private AllTranslationsQuery(IQueryable<TEntity> query)
        {
            _query = query;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static IIQueryableAllTranslationsExtensions<TEntity> Initialize(IQueryable<TEntity> query)
            => new AllTranslationsQuery<TEntity>(query);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public async Task<dynamic> FirstAsync(CancellationToken cancellationToken = default)
          => (await _query.GetAllTranslationsQuery(cancellationToken)).First();

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<dynamic> FirstAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => (await _query.Where(predicate).GetAllTranslationsQuery(cancellationToken)).First();

        //═════════════════════════════════════════════════════════════════════════════════════════

        public async Task<dynamic> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).FirstOrDefault();

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<dynamic> FirstOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => (await _query.Where(predicate).GetAllTranslationsQuery(cancellationToken)).FirstOrDefault();

        //═════════════════════════════════════════════════════════════════════════════════════════

        public async Task<dynamic> SingleAsync(CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).Single();

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<dynamic> SingleAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => (await _query.Where(predicate).GetAllTranslationsQuery(cancellationToken)).Single();

        //═════════════════════════════════════════════════════════════════════════════════════════

        public async Task<dynamic> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).SingleOrDefault();

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<dynamic> SingleOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => (await _query.Where(predicate).GetAllTranslationsQuery(cancellationToken)).SingleOrDefault();

        //═════════════════════════════════════════════════════════════════════════════════════════

        public async Task<dynamic[]> ToArrayAsync(CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).ToArray();

        //═════════════════════════════════════════════════════════════════════════════════════════

        public async Task<Dictionary<TKey, dynamic>> ToDictionaryAsync<TKey>([NotNull] Func<dynamic, TKey> keySelector, CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).ToDictionary(keySelector);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<Dictionary<TKey, dynamic>> ToDictionaryAsync<TKey>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).ToDictionary(keySelector, comparer);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] Func<dynamic, TElement> elementSelector, CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).ToDictionary(keySelector, elementSelector);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<dynamic, TKey> keySelector, [NotNull] Func<dynamic, TElement> elementSelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
            => (await _query.GetAllTranslationsQuery(cancellationToken)).ToDictionary(keySelector, elementSelector, comparer);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public async Task<List<dynamic>> ToListAsync(CancellationToken cancellationToken = default)
            => await _query.GetAllTranslationsQuery(cancellationToken);
    }
}
