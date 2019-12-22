using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public interface ITranslationQueryInitialized<TEntity>
    {
        IIQueryableExtensions<TEntity> WithFallback(params object[] parameters);
    }
}
