using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<TSource> ToTranslationTable<TSource>(this EntityTypeBuilder<TSource> builder, string name)
            where TSource : class
            => builder.AddAnnotation(TranslationAnnotationNames.Table, name);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> TranslationSchema<TSource>(this EntityTypeBuilder<TSource> builder, string name)
            where TSource : class
            => builder.AddAnnotation(TranslationAnnotationNames.Schema, name);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static EntityTypeBuilder<TSource> ToTranslationTable<TSource>(this EntityTypeBuilder<TSource> builder, string name, string schema)
            where TSource : class
        {
            builder.AddAnnotation(TranslationAnnotationNames.Table, name);
            builder.AddAnnotation(TranslationAnnotationNames.Schema, schema);
            return builder;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static EntityTypeBuilder<TSource> TranslationTableSuffix<TSource>(this EntityTypeBuilder<TSource> builder, string suffix)
            where TSource : class
            => builder.AddAnnotation(TranslationAnnotationNames.Suffix, suffix);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static EntityTypeBuilder<TSource> TranslationDeleteBehavior<TSource>(this EntityTypeBuilder<TSource> builder, DeleteBehavior deleteBehavior, 
            bool softDelete = false, IReadOnlyDictionary<string, object> onDeleteSetPropertyValue = null)
            where TSource : class
        {
            builder.AddAnnotation(TranslationAnnotationNames.SoftDelete, softDelete);
            builder.AddAnnotation(TranslationAnnotationNames.DeleteBehavior, deleteBehavior);
            return builder.AddAnnotation(TranslationAnnotationNames.OnSoftDeleteSetPropertyValue, onDeleteSetPropertyValue);
        }
    }
}
