using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationEntityConfigurationExtensions
    {
        public static EntityTypeBuilder<TSource> ToTranslationTable<TSource>(this EntityTypeBuilder<TSource> builder, string name)
            where TSource : class
            => builder.AddAnnotation("Table", name);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> TranslationSchema<TSource>(this EntityTypeBuilder<TSource> builder, string name)
            where TSource : class
            => builder.AddAnnotation("Schema", name);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> ToTranslationTable<TSource>(this EntityTypeBuilder<TSource> builder, string name, string schema)
            where TSource : class
        {
            builder.AddAnnotation("Table", name);
            builder.AddAnnotation("Schema", schema);
            return builder;
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> TranslationTableSuffix<TSource>(this EntityTypeBuilder<TSource> builder, string suffix)
            where TSource : class
            => builder.AddAnnotation("Suffix", suffix);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static EntityTypeBuilder<TSource> TranslationDeleteBehavior<TSource>(this EntityTypeBuilder<TSource> builder, DeleteBehavior deleteBehavior)
            where TSource : class
            => builder.AddAnnotation("DeleteBehavior", deleteBehavior);
    }
}
