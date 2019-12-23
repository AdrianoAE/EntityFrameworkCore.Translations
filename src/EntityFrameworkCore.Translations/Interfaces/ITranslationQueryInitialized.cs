namespace AdrianoAE.EntityFrameworkCore.Translations.Interfaces
{
    public interface ITranslationQueryInitialized<TEntity>
        where TEntity : class
    {
        IIQueryableExtensions<TEntity> WithFallback(params object[] parameters);
    }
}
