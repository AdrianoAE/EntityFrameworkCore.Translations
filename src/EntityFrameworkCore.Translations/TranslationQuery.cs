using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public sealed class TranslationQuery<TEntity> : IIQueryableExtensions<TEntity>, ITranslationQueryInitialized<TEntity>
        where TEntity : class
    {
        private IQueryable<TEntity> _query;
        private object[] _desiredParameters;
        private object[] _defaultParameters;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private TranslationQuery(IQueryable<TEntity> query, params object[] languageKey)
        {
            _query = query;
            _desiredParameters = languageKey;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static ITranslationQueryInitialized<TEntity> Initialize(IQueryable<TEntity> query, params object[] languageKey)
            => new TranslationQuery<TEntity>(query, languageKey);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public IIQueryableExtensions<TEntity> WithFallback(params object[] parameters)
        {
            _defaultParameters = parameters;
            return this;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public Task<TEntity> FirstAsync(CancellationToken cancellationToken = default)
          => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).FirstAsync(cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<TEntity> FirstAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters, predicate).FirstAsync(cancellationToken);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public Task<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).FirstOrDefaultAsync(cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<TEntity> FirstOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters, predicate).FirstOrDefaultAsync(cancellationToken);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public Task<TEntity> SingleAsync(CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).SingleAsync(cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<TEntity> SingleAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters, predicate).SingleAsync(cancellationToken);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public Task<TEntity> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).SingleOrDefaultAsync(cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<TEntity> SingleOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters, predicate).SingleOrDefaultAsync(cancellationToken);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public Task<TEntity[]> ToArrayAsync(CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToArrayAsync(cancellationToken);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>([NotNull] Func<TEntity, TKey> keySelector, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToDictionaryAsync(keySelector, cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToDictionaryAsync(keySelector, comparer, cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] Func<TEntity, TElement> elementSelector, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToDictionaryAsync(keySelector, elementSelector, cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>([NotNull] Func<TEntity, TKey> keySelector, [NotNull] Func<TEntity, TElement> elementSelector, [NotNull] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);

        //─────────────────────────────────────────────────────────────────────────────────────────

        public Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
            => _query.GetTranslatedQuery(_desiredParameters, _defaultParameters).ToListAsync(cancellationToken);
    }
}
