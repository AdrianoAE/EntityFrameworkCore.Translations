using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationConfiguration
    {
        private static readonly string _prefix = "_T_";
        private static readonly string _suffix = "Translations";
        private static readonly DeleteBehavior _deleteBehavior = DeleteBehavior.Cascade;
        private static readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        //─────────────────────────────────────────────────────────────────────────────────────────

        private static Dictionary<string, TranslationEntity> _translationEntities;
        internal static IReadOnlyDictionary<string, TranslationEntity> TranslationEntities => _translationEntities;

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static string Prefix { get; private set; } = _prefix;
        public static string Suffix { get; private set; } = _suffix;
        public static DeleteBehavior DeleteBehavior { get; private set; } = _deleteBehavior;
        public static IsolationLevel IsolationLevel { get; private set; } = _isolationLevel;
        public static LanguageTableConfiguration LanguageTableConfiguration { get; internal set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetPrefix(string prefix)
        {
            Prefix = !string.IsNullOrWhiteSpace(prefix) ? prefix : _prefix;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetSuffix(string suffix)
        {
            Prefix = !string.IsNullOrWhiteSpace(suffix) ? suffix : _suffix;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetDeleteBehavior(DeleteBehavior deleteBehavior)
        {
            DeleteBehavior = deleteBehavior;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static void SetTranslationEntities(Dictionary<string, TranslationEntity> translationEntities)
        {
            _translationEntities = translationEntities;
        }
    }
}
