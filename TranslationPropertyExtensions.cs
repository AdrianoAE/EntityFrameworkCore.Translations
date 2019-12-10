using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationPropertyExtensions
    {
        public static PropertyBuilder<string> PropertyWithTranslation<TSource>(this EntityTypeBuilder<TSource> builder, Expression<Func<TSource, string>> propertyExpression)
            where TSource : class
        {
            string propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;

            builder.Ignore(propertyName);

#pragma warning disable EF1001 // Internal EF Core API usage.
            return new PropertyBuilder<string>(builder.Property<string>($"{TranslationConfiguration.Prefix}{propertyName}").Metadata);
#pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}
