using AdrianoAE.EntityFrameworkCore.Translations.ShadowLanguageTable;
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
        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Type type, string foreignKeyName)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(type, foreignKeyName));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, string schema, Type type, string foreignKeyName)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(schema, type, foreignKeyName));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, IEnumerable<PrimaryKeyConfiguration> primaryKey)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(primaryKey));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, string schema, IEnumerable<PrimaryKeyConfiguration> primaryKey)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(schema, primaryKey));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, LanguageTableConfiguration configuration)
        {
            TranslationConfiguration.LanguageTableConfiguration = configuration;

            return modelBuilder.Configure();
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
            => modelBuilder.Configure(assembly.GetTypes().Where(t => t.IsClass && typeof(ILanguageEntity).IsAssignableFrom(t)).SingleOrDefault());

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Type languageEntity)
            => modelBuilder.Configure(languageEntity);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static ModelBuilder Configure(this ModelBuilder modelBuilder, Type languageEntity = null)
        {
            IMutableEntityType languageBuilder;

            foreach (var entity in new List<IMutableEntityType>(modelBuilder.Model.GetEntityTypes()))
            {
                languageBuilder = null;

                var propertiesToIgnore = new List<string>();
                var propertiesWithTranslation = entity.GetProperties().Where(p => p.Name.Length > TranslationConfiguration.Prefix.Length
                    && p.Name.Substring(0, TranslationConfiguration.Prefix.Length).Contains(TranslationConfiguration.Prefix));

                if (propertiesWithTranslation.Count() > 0)
                {
                    modelBuilder.Entity<Translation>(translationConfiguration =>
                    {
                        translationConfiguration.ToTable(entity.GetTranslationTableName(), entity.GetSchemaName());

                        if (languageEntity != null)
                        {
                            languageBuilder = modelBuilder.Entity(languageEntity).Metadata;
                        }

                        translationConfiguration.ConfigureKeys(entity, languageBuilder);

                        foreach (var property in propertiesWithTranslation)
                        {
                            propertiesToIgnore.Add(property.Name);
                            translationConfiguration.ConfigureProperty(property);
                        }
                    });

                    foreach (var property in propertiesToIgnore)
                    {
                        modelBuilder.Entity(entity.Name).Ignore(property);
                    }
                }
            }

            return modelBuilder;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static string GetTranslationTableName(this IMutableEntityType entity)
            => entity.FindAnnotation($"{TranslationConfiguration.Prefix}Table")?.Value.ToString()
                ?? entity.GetTableName() +
                    (entity.FindAnnotation($"{TranslationConfiguration.Prefix}Suffix")?.Value
                    ?? TranslationConfiguration.Suffix);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static string GetSchemaName(this IMutableEntityType entity)
            => entity.FindAnnotation($"{TranslationConfiguration.Prefix}Schema")?.Value.ToString()
                ?? TranslationConfiguration.LanguageTableConfiguration?.TranslationsSchema
                ?? entity.GetSchema();

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureKeys(this EntityTypeBuilder<Translation> builder, IMutableEntityType entity, IMutableEntityType languageBuilder)
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
            if (languageBuilder == null)
            {
                foreach (var key in TranslationConfiguration.LanguageTableConfiguration.PrimaryKey)
                {
                    builder.Property(key.Type, key.ForeignKeyName);
                    primaryKeys.Add(key.ForeignKeyName);
                }
            }
            else if (languageBuilder != null)
            {
                var foreignKeys = new List<string>();

                foreach (var property in languageBuilder.GetProperties().Where(p => p.IsPrimaryKey()))
                {
                    string name = $"{languageBuilder.ClrType.Name}{property.GetColumnName()}";

                    primaryKeys.Add(name);
                    foreignKeys.Add(name);

                    builder.Property(property.ClrType, name)
                        .HasColumnType(property.GetColumnType());
                }

                builder.HasOne(languageBuilder.ClrType)
                    .WithMany()
                    .HasForeignKey(foreignKeys.ToArray())
                    .OnDelete(deleteBehavior);
            }

            builder.HasKey(primaryKeys.ToArray());
        }
    }
}
