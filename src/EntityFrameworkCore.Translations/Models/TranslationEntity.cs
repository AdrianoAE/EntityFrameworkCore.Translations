using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    internal class TranslationEntity
    {
        internal Type Type { get; private set; }
        internal string Schema { get; set; }
        internal string TableName { get; set; }
        internal DeleteBehavior DeleteBehavior { get; set; }
        internal bool SoftDelete { get; set; }
        internal IDictionary<string, object> OnSoftDeleteSetPropertyValue { get; set; }
        internal IDictionary<string, string> KeysFromSourceEntity { get; private set; }
        internal ICollection<KeyConfiguration> KeysFromLanguageEntity { get; private set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal TranslationEntity(Type type)
        {
            Type = type;
            KeysFromSourceEntity = new Dictionary<string, string>();
            KeysFromLanguageEntity = new List<KeyConfiguration>();
        }
    }
}
