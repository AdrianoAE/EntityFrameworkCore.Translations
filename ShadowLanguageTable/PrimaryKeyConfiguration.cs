using System;

namespace AdrianoAE.EntityFrameworkCore.Translations.ShadowLanguageTable
{
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
