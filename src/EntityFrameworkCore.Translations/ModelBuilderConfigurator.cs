using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    internal static class ModelBuilderConfigurator
    {
        private static MethodInfo configureEntityMethod => typeof(ModelBuilderConfigurator)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(t => t.IsGenericMethod && t.Name == nameof(ConfigureEntity));

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static ModelBuilder Configure(this ModelBuilder modelBuilder, Type languageEntity = null)
        {
            IMutableEntityType languageBuilder = null;

            InitializeTranslationEntities();

            foreach (var entity in new List<IMutableEntityType>(modelBuilder.Model.GetEntityTypes()))
            {
                TranslationConfiguration.TranslationEntities.TryGetValue(entity.ClrType.FullName, out TranslationEntity translationEntity);

                if (translationEntity != null)
                {
                    var propertiesWithTranslation = entity.GetProperties()
                        .Where(efproperty => translationEntity.Type.GetProperties()
                            .Where(property => property.PropertyType == typeof(string) && entity.ClrType.GetProperty(property.Name) != null)
                            .Select(p => p.Name)
                            .Contains(efproperty.Name))
                        .ToList();

                    if (propertiesWithTranslation.Count > 0)
                    {
                        var method = configureEntityMethod.MakeGenericMethod(translationEntity.Type);

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
               .ToDictionary(type => type.GetInterface("ITranslation`1").GetGenericArguments()[0].FullName, type => new TranslationEntity(type));

            TranslationConfiguration.SetTranslationEntities(translationEntities);
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static string GetTranslationTableName(this IMutableEntityType entity)
            => entity.FindAnnotation($"{TranslationAnnotationNames.Table}")?.Value.ToString()
                ?? entity.GetTableName() +
                    (entity.FindAnnotation($"{TranslationAnnotationNames.Suffix}")?.Value
                    ?? TranslationConfiguration.Suffix);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static string GetSchemaName(this IMutableEntityType entity)
            => entity.FindAnnotation($"{TranslationAnnotationNames.Schema}")?.Value.ToString()
                ?? TranslationConfiguration.LanguageTableConfiguration?.TranslationsSchema
                ?? entity.GetSchema();

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureEntity<TType>(ModelBuilder modelBuilder, IMutableEntityType entity, Type languageEntity, 
            IMutableEntityType languageBuilder, List<IMutableProperty> propertiesWithTranslation)
            where TType : class
        {
            var table = entity.GetTranslationTableName();
            var schema = entity.GetSchemaName();
            var configuration = TranslationConfiguration.TranslationEntities[entity.ClrType.FullName];

            configuration.Schema = schema;
            configuration.TableName = table;
            configuration.SoftDelete = (bool)(entity.FindAnnotation(TranslationAnnotationNames.SoftDelete)?.Value ?? TranslationConfiguration.SoftDelete);
            configuration.DeleteBehavior = (DeleteBehavior)(entity.FindAnnotation(TranslationAnnotationNames.DeleteBehavior)?.Value ?? TranslationConfiguration.DeleteBehavior);
            configuration.OnSoftDeleteSetPropertyValue = (Dictionary<string, object>)entity.FindAnnotation(TranslationAnnotationNames.OnSoftDeleteSetPropertyValue)?.Value 
                ?? (Dictionary<string, object>)TranslationConfiguration.OnSoftDeleteSetPropertyValue;

            modelBuilder.Entity<TType>(translationConfiguration =>
            {
                translationConfiguration.ToTable(table, schema);

                if (languageEntity != null)
                {
                    languageBuilder = modelBuilder.Entity(languageEntity).Metadata;
                }

                translationConfiguration.ConfigureKeys(entity, languageBuilder, configuration);

                foreach (var property in propertiesWithTranslation)
                {
                    translationConfiguration.ConfigureProperty(property);
                }
            });
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureKeys(this EntityTypeBuilder builder, IMutableEntityType entity, IMutableEntityType languageBuilder, TranslationEntity configuration)
        {
            var primaryKeys = new List<string>();

            //Source Table
            foreach (var key in entity.GetProperties().Where(p => p.IsPrimaryKey()))
            {
                string name = $"{entity.ClrType.Name}{key.GetColumnName()}";

                builder.Property(key.ClrType, name)
                    .HasColumnType(key.GetColumnType());

                primaryKeys.Add(name);
                configuration.KeysFromSourceEntity.Add(key.GetColumnName(), name);
            }

            builder.HasOne(entity.ClrType)
                .WithMany()
                .HasForeignKey(primaryKeys.ToArray())
                .OnDelete(configuration.DeleteBehavior);

            //Language Table
            if (languageBuilder == null)
            {
                foreach (var key in TranslationConfiguration.LanguageTableConfiguration.PrimaryKey)
                {
                    builder.Property(key.Type, key.Name);

                    primaryKeys.Add(key.Name);
                    configuration.KeysFromLanguageEntity.Add(new KeyConfiguration(key.Type, key.Name));
                }
            }
            else
            {
                var foreignKeys = new List<KeyConfiguration>();

                foreach (var property in languageBuilder.GetProperties().Where(p => p.IsPrimaryKey()))
                {
                    string name = $"{languageBuilder.ClrType.Name}{property.GetColumnName()}";

                    builder.Property(property.ClrType, name)
                        .HasColumnType(property.GetColumnType());

                    primaryKeys.Add(name);
                    foreignKeys.Add(new KeyConfiguration(property.ClrType, name));
                    configuration.KeysFromLanguageEntity.Add(new KeyConfiguration(property.ClrType, name));
                }

                builder.HasOne(languageBuilder.ClrType)
                    .WithMany()
                    .HasForeignKey(foreignKeys.Select(fk => fk.Name).ToArray())
                    .OnDelete(configuration.DeleteBehavior);

                TranslationConfiguration.LanguageTableConfiguration = new LanguageTableConfiguration(foreignKeys);
            }

            builder.HasKey(primaryKeys.ToArray());
        }
    }
}
