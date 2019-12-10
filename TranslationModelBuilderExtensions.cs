using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public static class TranslationModelBuilderExtensions
    {
        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
        {
            Type entity = null;

            entity = assembly.GetTypes().Where(t => typeof(ILanguageEntity).IsAssignableFrom(t)).SingleOrDefault();

            return ApplyTranslationsConfigurations(modelBuilder, entity);
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Type languageEntity = null)
        {
            IMutableEntityType languagueBuilder = null;

            foreach (var entity in new List<IMutableEntityType>(modelBuilder.Model.GetEntityTypes()))
            {
                var propertiesToIgnore = new Dictionary<string, string>();
                var propertiesWithTranslation = entity.GetProperties().Where(p => p.Name.Length > TranslationConfiguration.Prefix.Length
                    && p.Name.Substring(0, TranslationConfiguration.Prefix.Length).Contains(TranslationConfiguration.Prefix));

                if (propertiesWithTranslation.Count() > 0)
                {
                    modelBuilder.Entity<Translation>(translationConfiguration =>
                    {
                        translationConfiguration.ToTable(entity.GetTranslationTableName());

                        if (languageEntity != null)
                        {
                            languagueBuilder = modelBuilder.Entity(languageEntity).Metadata;
                        }

                        translationConfiguration.ConfigureKeys(entity, languagueBuilder);

                        foreach (var property in propertiesWithTranslation)
                        {
                            propertiesToIgnore.Add(entity.Name, property.Name);
                            translationConfiguration.ConfigureProperty(property);
                        }
                    });

                    foreach (var property in propertiesToIgnore)
                    {
                        modelBuilder.Entity(entity.Name).Ignore(property.Value);
                    }
                }
            }

            return modelBuilder;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static string GetTranslationTableName(this IMutableEntityType entity)
            => entity.FindAnnotation($"{TranslationConfiguration.Prefix}Table")?.Value.ToString()
                ?? entity.GetTableName() +
                        entity.FindAnnotation($"{TranslationConfiguration.Prefix}Suffix")?.Value
                        ?? TranslationConfiguration.Suffix;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureKeys(this EntityTypeBuilder<Translation> builder, IMutableEntityType entity, IMutableEntityType languagueBuilder)
        {
            var primaryKeys = new List<string>();
            var deleteBehavior = (DeleteBehavior)(entity.FindAnnotation($"{TranslationConfiguration.Prefix}DeleteBehavior")?.Value ?? TranslationConfiguration.DeleteBehavior);

            //Source Table
            foreach (var key in entity.GetProperties().Where(p => p.IsPrimaryKey()))
            {
                string name = $"{entity.ClrType.Name}{key.GetColumnName()}";

                primaryKeys.Add(name);

                builder.Property(key.ClrType, name)
                    .HasColumnType(key.GetColumnType());
            }

            builder.HasOne(entity.ClrType)
                .WithMany()
                .HasForeignKey(primaryKeys.ToArray())
                .OnDelete(deleteBehavior);

            //Language Table
            if (languagueBuilder == null)
            {
                var languageConfigurations = (IEnumerable<LanguageTableConfiguration>)entity.FindAnnotation($"{TranslationConfiguration.Prefix}LanguageShadowTable")?.Value;

                if (languageConfigurations?.Count() > 0)
                {
                    foreach (var configuration in languageConfigurations)
                    {
                        builder.Property(configuration.Type, configuration.ForeignKeyName);
                        primaryKeys.Add(configuration.ForeignKeyName);
                    }
                }
                else
                {
                    throw new ArgumentNullException("ERRRRRRRRRRROUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
                }
            }
            else
            {
                var foreignKeys = new List<string>();

                foreach (var property in languagueBuilder.GetProperties().Where(p => p.IsPrimaryKey()))
                {
                    string name = $"{languagueBuilder.ClrType.Name}{property.GetColumnName()}";

                    primaryKeys.Add(name);
                    foreignKeys.Add(name);

                    builder.Property(property.ClrType, name)
                        .HasColumnType(property.GetColumnType());
                }

                builder.HasOne(languagueBuilder.ClrType)
                    .WithMany()
                    .HasForeignKey(foreignKeys.ToArray())
                    .OnDelete(deleteBehavior);
            }

            builder.HasKey(primaryKeys.ToArray());
        }
    }
}
