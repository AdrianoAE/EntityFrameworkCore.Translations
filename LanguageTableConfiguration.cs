using System;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public class LanguageTableConfiguration
    {
        public Type Type { get; private set; }
        public string ForeignKeyName { get; private set; }

        public LanguageTableConfiguration(Type type, string foreignKeyName)
        {
            Type = type;
            ForeignKeyName = !string.IsNullOrWhiteSpace(foreignKeyName) ? foreignKeyName : throw new ArgumentNullException(nameof(ForeignKeyName));
        }
    }
}
