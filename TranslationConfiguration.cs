using Microsoft.EntityFrameworkCore;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationConfiguration
    {
        private static string _prefix = "_T_";
        private static string _suffix = "Translations";
        private static DeleteBehavior _deleteBehavior = DeleteBehavior.Cascade;

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
    }
}
