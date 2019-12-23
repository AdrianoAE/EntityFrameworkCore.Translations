using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class IQueryableExtensions
    {
        public static ITranslationQueryInitialized<TEntity> WithLanguage<TEntity>([NotNull] this IQueryable<TEntity> source, params object[] parameters)
            where TEntity : class
            => TranslationQuery<TEntity>.Initialize(source, parameters);
    }
}
