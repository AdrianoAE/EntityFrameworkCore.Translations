using System;
using System.Collections.Generic;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public class LanguageTableConfiguration
    {
        public readonly string TranslationsSchema;
        public readonly IReadOnlyCollection<PrimaryKeyConfiguration> PrimaryKey;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public LanguageTableConfiguration(Type type, string foreignKeyName)
            : this(new PrimaryKeyConfiguration(type, foreignKeyName))
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(PrimaryKeyConfiguration primaryKey)
            : this(new List<PrimaryKeyConfiguration> { primaryKey })
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(IEnumerable<PrimaryKeyConfiguration> primaryKey)
        {
            TranslationsSchema = null;
            PrimaryKey = primaryKey.Count() > 0 ? primaryKey.ToList().AsReadOnly() : throw new ArgumentNullException(nameof(PrimaryKey), "At least one key is required.");
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, Type type, string foreignKeyName)
            : this(schema, new PrimaryKeyConfiguration(type, foreignKeyName))
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, PrimaryKeyConfiguration primaryKey)
            : this(schema, new List<PrimaryKeyConfiguration> { primaryKey })
        { }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public LanguageTableConfiguration(string schema, IEnumerable<PrimaryKeyConfiguration> primaryKey)
        {
            TranslationsSchema = !string.IsNullOrWhiteSpace(schema) ? schema : throw new ArgumentNullException(nameof(TranslationsSchema));
            PrimaryKey = primaryKey.Count() > 0 ? primaryKey.ToList().AsReadOnly() : throw new ArgumentNullException(nameof(PrimaryKey), "At least one key is required.");
        }
    }

    public class PrimaryKeyConfiguration
    {
        public readonly Type Type;
        public readonly string ForeignKeyName;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public PrimaryKeyConfiguration(Type type, string foreignKeyName)
        {
            Type = type;
            ForeignKeyName = !string.IsNullOrWhiteSpace(foreignKeyName) ? foreignKeyName : throw new ArgumentNullException(nameof(ForeignKeyName));
        }
    }
}
