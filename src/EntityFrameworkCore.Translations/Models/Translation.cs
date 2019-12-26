namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    public class Translation<TEntity>
        where TEntity : class
    {
        internal readonly TEntity Entity;
        internal readonly object[] LanguageKey;

        public Translation(TEntity translatedEntity, params object[] languageKey)
        {
            Entity = translatedEntity;
            LanguageKey = languageKey;
        }
    }
}
