using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationConfiguration
    {
        private const string _suffix = "Translations";
        private const bool _softDelete = false;
        private const DeleteBehavior _deleteBehavior = DeleteBehavior.Cascade;
        private const IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        //─────────────────────────────────────────────────────────────────────────────────────────

        private static Dictionary<string, TranslationEntity> _translationEntities;
        internal static IReadOnlyDictionary<string, TranslationEntity> TranslationEntities => _translationEntities;
        internal static IReadOnlyDictionary<string, object> OnDeleteSetPropertyValue { get; private set; }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static string Suffix { get; private set; } = _suffix;
        public static bool SoftDelete { get; private set; } = _softDelete;
        public static DeleteBehavior DeleteBehavior { get; private set; } = _deleteBehavior;
        public static IsolationLevel IsolationLevel { get; private set; } = _isolationLevel;
        public static LanguageTableConfiguration LanguageTableConfiguration { get; internal set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetSuffix(string suffix) 
            => Suffix = !string.IsNullOrWhiteSpace(suffix) ? suffix : _suffix;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetDeleteBehavior(DeleteBehavior deleteBehavior, bool softDelete = false, IReadOnlyDictionary<string, object> onDeleteSetPropertyValue = null)
        {
            DeleteBehavior = deleteBehavior;
            SoftDelete = softDelete;
            OnDeleteSetPropertyValue = onDeleteSetPropertyValue;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static void SetTranslationEntities(Dictionary<string, TranslationEntity> translationEntities) 
            => _translationEntities = translationEntities;
    }
}
