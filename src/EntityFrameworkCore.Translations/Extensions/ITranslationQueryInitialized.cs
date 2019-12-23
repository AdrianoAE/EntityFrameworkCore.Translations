using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public interface ITranslationQueryInitialized<TEntity>
        where TEntity : class
    {
        IIQueryableExtensions<TEntity> WithFallback(params object[] parameters);
    }
}
