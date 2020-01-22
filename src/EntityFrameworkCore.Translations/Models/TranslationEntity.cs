using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    public class TranslationEntity
    {
        internal Type Type { get; private set; }
        internal string Schema { get; set; }
        internal string TableName { get; set; }
        internal DeleteBehavior DeleteBehavior { get; set; }
        internal bool SoftDelete { get; set; }
        internal Dictionary<string, object> OnSoftDeleteSetPropertyValue { get; set; }
        internal Dictionary<string, string> KeysFromSourceEntity { get; private set; }
        internal List<KeyConfiguration> KeysFromLanguageEntity { get; private set; }

        public IReadOnlyDictionary<string, string> KeysFromSource => KeysFromSourceEntity;
        public IReadOnlyCollection<KeyConfiguration> KeysFromLanguage => KeysFromLanguageEntity;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal TranslationEntity(Type type)
        {
            Type = type;
            KeysFromSourceEntity = new Dictionary<string, string>();
            KeysFromLanguageEntity = new List<KeyConfiguration>();
        }
    }
}
