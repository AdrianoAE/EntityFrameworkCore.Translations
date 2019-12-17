using AdrianoAE.EntityFrameworkCore.Translations.ShadowLanguageTable;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationConfiguration
    {
        private static readonly string _prefix = "_T_";
        private static readonly string _suffix = "Translations";
        private static readonly DeleteBehavior _deleteBehavior = DeleteBehavior.Cascade;

        //─────────────────────────────────────────────────────────────────────────────────────────

        private static Dictionary<string, Type> _translationEntities;
        internal static IReadOnlyDictionary<string, Type> TranslationEntities => _translationEntities;

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static string Prefix { get; private set; } = _prefix;
        public static string Suffix { get; private set; } = _suffix;
        public static DeleteBehavior DeleteBehavior { get; private set; } = _deleteBehavior;
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

        internal static void SetTranslationEntities(Dictionary<string, Type> translationEntities)
        {
            _translationEntities = translationEntities;
        }
    }
}
