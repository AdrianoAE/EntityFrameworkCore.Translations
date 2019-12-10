using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationEntityConfigurationExtensions
    {
        public static EntityTypeBuilder<TSource> ToTranslationTable<TSource>(this EntityTypeBuilder<TSource> builder, string name)
            where TSource : class
            => builder.AddAnnotation("Table", name);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> TranslationTableSuffix<TSource>(this EntityTypeBuilder<TSource> builder, string suffix)
            where TSource : class
            => builder.AddAnnotation("Suffix", suffix);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static EntityTypeBuilder<TSource> TranslationDeleteBehavior<TSource>(this EntityTypeBuilder<TSource> builder, DeleteBehavior deleteBehavior)
            where TSource : class
            => builder.AddAnnotation("DeleteBehavior", deleteBehavior);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static EntityTypeBuilder<TSource> LanguageShadowTable<TSource>(this EntityTypeBuilder<TSource> builder, Type type, string foreignKeyName)
            where TSource : class
            => LanguageShadowTable(builder, new LanguageTableConfiguration(type, foreignKeyName));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static EntityTypeBuilder<TSource> LanguageShadowTable<TSource>(this EntityTypeBuilder<TSource> builder, LanguageTableConfiguration configuration)
            where TSource : class
            => LanguageShadowTable(builder, new List<LanguageTableConfiguration> { configuration });

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static EntityTypeBuilder<TSource> LanguageShadowTable<TSource>(this EntityTypeBuilder<TSource> builder, IEnumerable<LanguageTableConfiguration> configuration)
            where TSource : class
            => builder.AddAnnotation("LanguageShadowTable", configuration);
    }
}
