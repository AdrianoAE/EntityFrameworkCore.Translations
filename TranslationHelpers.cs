using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    internal static class TranslationHelpers
    {
        public static EntityTypeBuilder<TSource> AddAnnotation<TSource>(this EntityTypeBuilder<TSource> builder, string name, object value)
            where TSource : class
        {
            string annotationName = $"{TranslationConfiguration.Prefix}{name}";

            var existingAnnotation = builder.Metadata.FindAnnotation(annotationName);

            if (existingAnnotation == null)
            {
                builder.Metadata.AddAnnotation(annotationName, value);
            }
            else
            {
                builder.Metadata.SetAnnotation(annotationName, value);
            }

            return builder;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static PropertyBuilder<string> ConfigureProperty<TSource>(this EntityTypeBuilder<TSource> builder, IMutableProperty property)
            where TSource : class
        {
            var newProperty = builder.Property<string>(property.Name.Replace(TranslationConfiguration.Prefix, "")).Metadata;

            foreach (var item in property.GetAnnotations())
            {
                newProperty.SetAnnotation(item.Name, item.Value);
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            return new PropertyBuilder<string>(newProperty).IsRequired(!property.IsNullable);
#pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}
