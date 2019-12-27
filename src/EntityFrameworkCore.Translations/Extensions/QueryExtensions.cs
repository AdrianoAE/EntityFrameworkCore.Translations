using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class QueryExtensions
    {
        public static ITranslationQueryInitialized<TEntity> WithLanguage<TEntity>([NotNull] this IQueryable<TEntity> source, params object[] languageKey)
            where TEntity : class
            => TranslationQuery<TEntity>.Initialize(source, languageKey);
    }
}
