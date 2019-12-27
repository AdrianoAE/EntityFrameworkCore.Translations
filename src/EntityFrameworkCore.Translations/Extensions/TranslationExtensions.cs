namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    public static class TranslationExtensions
    {
        public static Translation<TEntity> Create<TEntity>(TEntity translatedEntity, params object[] languageKey)
            where TEntity : class
            => new Translation<TEntity>(translatedEntity, languageKey);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static Translation<TEntity> TranslationOf<TEntity>(this TEntity translatedEntity, params object[] languageKey)
            where TEntity : class
            => Create(translatedEntity, languageKey);
    }
}
