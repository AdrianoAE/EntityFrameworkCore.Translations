using Microsoft.EntityFrameworkCore;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationConfiguration
    {
        private static string _prefix = "_T_";
        private static string _suffix = "Translations";
        private static DeleteBehavior _deleteBehavior = DeleteBehavior.Cascade;

        public static string Prefix { get; private set; }
        public static string Suffix { get; private set; }
        public static DeleteBehavior DeleteBehavior { get; private set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        static TranslationConfiguration()
        {
            SetPrefix(_prefix);
            SetSuffix(_suffix);
            SetDeleteBehavior(_deleteBehavior);
        }

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
