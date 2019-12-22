using System;
using System.Collections.Generic;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    public class LanguageTableConfiguration
    {
        public readonly string TranslationsSchema;
        public readonly IReadOnlyCollection<KeyConfiguration> PrimaryKey;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public LanguageTableConfiguration(Type type, string foreignKeyName)
            : this(new KeyConfiguration(type, foreignKeyName))
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(KeyConfiguration primaryKey)
            : this(new List<KeyConfiguration> { primaryKey })
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(IEnumerable<KeyConfiguration> primaryKey)
        {
            TranslationsSchema = null;
            PrimaryKey = primaryKey.Count() > 0 ? primaryKey.ToList().AsReadOnly() : throw new ArgumentNullException(nameof(PrimaryKey), "At least one key is required.");
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, Type type, string foreignKeyName)
            : this(schema, new KeyConfiguration(type, foreignKeyName))
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, KeyConfiguration primaryKey)
            : this(schema, new List<KeyConfiguration> { primaryKey })
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, IEnumerable<KeyConfiguration> primaryKey)
        {
            TranslationsSchema = !string.IsNullOrWhiteSpace(schema) ? schema : throw new ArgumentNullException(nameof(TranslationsSchema));
            PrimaryKey = primaryKey.Count() > 0 ? primaryKey.ToList().AsReadOnly() : throw new ArgumentNullException(nameof(PrimaryKey), "At least one key is required.");
        }
    }
}
