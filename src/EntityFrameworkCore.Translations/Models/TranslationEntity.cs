using System;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    internal class TranslationEntity
    {
        internal Type Type { get; private set; }
        internal ICollection<KeyConfiguration> KeysFromLanguageEntity { get; private set; }
        internal IDictionary<string, string> KeysFromSourceEntity { get; private set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal TranslationEntity(Type type)
        {
            Type = type;
            KeysFromLanguageEntity = new List<KeyConfiguration>();
            KeysFromSourceEntity = new Dictionary<string, string>();
        }
    }
}
