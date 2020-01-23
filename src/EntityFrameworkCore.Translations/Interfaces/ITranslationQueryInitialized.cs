namespace AdrianoAE.EntityFrameworkCore.Translations.Interfaces
{
    public interface ITranslationQueryInitialized<TEntity>
        where TEntity : class
    {
        IIQueryableTranslationExtensions<TEntity> WithFallback(params object[] parameters);
    }
}
