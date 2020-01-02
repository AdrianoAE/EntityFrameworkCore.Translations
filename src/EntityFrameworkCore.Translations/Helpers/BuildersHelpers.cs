using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdrianoAE.EntityFrameworkCore.Translations.Helpers
{
    internal static class BuildersHelpers
    {
        internal static EntityTypeBuilder<TSource> AddAnnotation<TSource>(this EntityTypeBuilder<TSource> builder, string annotationName, object value)
            where TSource : class
        {
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

        internal static PropertyBuilder<string> ConfigureProperty(this EntityTypeBuilder builder, IMutableProperty property)
        {
            var newProperty = builder.Property<string>(property.Name.Replace(TranslationAnnotationNames.Prefix, string.Empty)).Metadata;

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
