using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdrianoAE.EntityFrameworkCore.Translations.Helpers
{
    internal static class ModelBuilderHelper
    {
        internal static ModelBuilder Configure(this ModelBuilder modelBuilder, Type languageEntity = null)
        {
            IMutableEntityType languageBuilder = null;

            InitializeTranslationEntities();

            if (TranslationConfiguration.TranslationEntities.Count == 0)
            {
                return modelBuilder;
            }

            MethodInfo ConfigureEntityMethod = typeof(ModelBuilderHelper)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(t => t.IsGenericMethod && t.Name == nameof(ConfigureEntity));

            foreach (var entity in new List<IMutableEntityType>(modelBuilder.Model.GetEntityTypes()))
            {
                TranslationConfiguration.TranslationEntities.TryGetValue(entity.ClrType.FullName, out Type translationType);

                if (translationType != null)
                {
                    var propertiesWithTranslation = entity.GetProperties()
                        .Where(property => translationType.GetProperties().Select(p => p.Name).Contains(property.Name))
                        .ToList();

                    if (propertiesWithTranslation.Count > 0)
                    {
                        var method = ConfigureEntityMethod.MakeGenericMethod(translationType);

                        method.Invoke(null, new object[] { modelBuilder, entity, languageEntity, languageBuilder, propertiesWithTranslation });

                        foreach (var property in propertiesWithTranslation)
                        {
                            modelBuilder.Entity(entity.Name).Ignore(property.Name);
                        }
                    }
                }
            }

            return modelBuilder;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void InitializeTranslationEntities()
        {
            if (TranslationConfiguration.TranslationEntities == null)
            {
                var translationEntities = AppDomain.CurrentDomain
                   .GetAssemblies()
                   .Where(assembly =>
                   {
                       var value = assembly.CustomAttributes
                           .FirstOrDefault(attribute => attribute.AttributeType == typeof(AssemblyProductAttribute))
                           ?.ConstructorArguments[0].Value as string;

                       return value != null ? !value.Contains("Microsoft") : false;
                   })
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITranslation<>)))
                   .ToDictionary(type => type.GetInterface("ITranslation`1").GetGenericArguments()[0].FullName, type => type);

                TranslationConfiguration.SetTranslationEntities(translationEntities);
            }
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

        private static void ConfigureEntity<TType>(ModelBuilder modelBuilder, IMutableEntityType entity, Type languageEntity, IMutableEntityType languageBuilder, List<IMutableProperty> propertiesWithTranslation)
            where TType : class
        {
            modelBuilder.Entity<TType>(translationConfiguration =>
            {
                translationConfiguration.ToTable(entity.GetTranslationTableName(), entity.GetSchemaName());

                if (languageEntity != null)
                {
                    languageBuilder = modelBuilder.Entity(languageEntity).Metadata;
                }

                translationConfiguration.ConfigureKeys(entity, languageBuilder);

                foreach (var property in propertiesWithTranslation)
                {
                    translationConfiguration.ConfigureProperty(property);
                }
            });
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureKeys(this EntityTypeBuilder builder, IMutableEntityType entity, IMutableEntityType languageBuilder)
        {
            var entityPrimaryKeys = entity.GetProperties()
                .Where(property => property.IsPrimaryKey())
                .Select(property => $"{entity.ClrType.Name}{property.GetColumnName()}");

            var primaryKeys = new List<string>(entityPrimaryKeys);
            var deleteBehavior = (DeleteBehavior)(entity.FindAnnotation($"{TranslationConfiguration.Prefix}DeleteBehavior")?.Value ?? TranslationConfiguration.DeleteBehavior);

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
