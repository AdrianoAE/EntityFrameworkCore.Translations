using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class CommandExtensions
    {
        private static MethodInfo getExistingTranslationsMethod => typeof(CommandExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(t => t.IsGenericMethod && t.Name == nameof(GetExistingTranslations));

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void UpsertTranslation<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity entity, TEntity translation, params object[] languageKey)
            where TEntity : class
            => UpsertTranslationRange(source, entity, new Translation<TEntity>(translation, languageKey));

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static void UpsertTranslation<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity entity, Translation<TEntity> translationEntity)
            where TEntity : class
            => UpsertTranslationRange(source, entity, translationEntity);

        //═════════════════════════════════════════════════════════════════════════════════════════

        public static void UpsertTranslationRange<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity entity, params Translation<TEntity>[] translationEntities)
            where TEntity : class
        {
            int parameterPosition;
            var context = PersistenceHelpers.GetDbContext(source);
            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];
            var method = getExistingTranslationsMethod.MakeGenericMethod(typeof(TEntity), translationEntity.Type);
            IEnumerable<IDictionary<string, object>> existingTranslations = null;

            PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, translationEntities.SelectMany(translation => translation.LanguageKey).ToArray());

            foreach (var entry in translationEntities)
            {
                var translation = Activator.CreateInstance(translationEntity.Type);
                var trackedEntity = context.ChangeTracker.Entries().Where(entry => entry.Entity.GetType() == translationEntity.Type);

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    translationEntity.Type.GetProperty(property.Name)?.SetValue(translation, property.GetValue(entry.Entity));
                }

                parameterPosition = 0;
                foreach (var property in translationEntity.KeysFromLanguageEntity)
                {
                    context.Entry(translation).Property(property.Name).CurrentValue = entry.LanguageKey[parameterPosition];
                    trackedEntity = trackedEntity.Where(entry => entry.Property(property.Name).CurrentValue.Equals(context.Entry(translation).Property(property.Name).CurrentValue));
                    parameterPosition++;
                }

                foreach (var property in translationEntity.KeysFromSourceEntity)
                {
                    context.Entry(translation).Property(property.Value).CurrentValue = entity.GetType().GetProperty(property.Key).GetValue(entity);
                    trackedEntity = trackedEntity.Where(entry => entry.Property(property.Value).CurrentValue.Equals(context.Entry(translation).Property(property.Value).CurrentValue));
                }

                var tracked = trackedEntity.SingleOrDefault();
                if (tracked != null)
                {
                    foreach (var property in tracked.Entity.GetType().GetProperties()
                        .Where(property => !translationEntity.KeysFromLanguageEntity.Select(key => key.Name).Contains(property.Name)
                           || !translationEntity.KeysFromSourceEntity.Select(key => key.Value).Contains(property.Name)))
                    {
                        tracked.Property(property.Name).CurrentValue = context.Entry(translation).Property(property.Name).CurrentValue;
                    }
                }
                else
                {
                    existingTranslations ??= ((IEnumerable<IDictionary<string, object>>)method.Invoke(null, new object[] { context, entity, translationEntity, translationEntities })).ToList();

                    var existingTranslation = existingTranslations.AsQueryable();
                    foreach (var property in context.Entry(translation).Properties
                        .Where(property => translationEntity.KeysFromLanguageEntity.Select(key => key.Name).Contains(property.Metadata.Name)
                            || translationEntity.KeysFromSourceEntity.Select(key => key.Value).Contains(property.Metadata.Name)))
                    {
                        existingTranslation = existingTranslation.Where(translation => translation[property.Metadata.Name].Equals(property.CurrentValue));
                    }

                    if (existingTranslation.Count() > 0)
                    {
                        context.Entry(translation).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Entry(translation).State = EntityState.Added;
                    }
                }
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static IEnumerable<IDictionary<string, object>> GetExistingTranslations<TEntity, TTranslatedEntity>(this DbContext context, TEntity entity,
            TranslationEntity translationEntity, Translation<TEntity>[] translationEntities)
            where TEntity : class
            where TTranslatedEntity : class
        {
            var schema = !string.IsNullOrWhiteSpace(translationEntity.Schema) ? $"[{translationEntity.Schema}]." : string.Empty;

            var query = new StringBuilder();
            query.Append("SELECT ");
            query.Append(string.Join(" ,", context.Model.FindEntityType(translationEntity.Type).GetProperties().Select(property => $"[t].[{property.GetColumnName()}]")));
            query.Append($" FROM {schema}[{translationEntity.TableName}] AS [t]");
            query.Append(" WHERE ");
            query.Append(string.Join(" AND ", translationEntity.KeysFromSourceEntity
                .Select(property => $"[t].[{property.Value}] = @{property.Value}")));
            query.Append(" AND (");
            query.Append(string.Join(" OR ", translationEntities
                .Select((translation, index) => string.Join(" ,", translationEntity.KeysFromLanguageEntity
                    .Select(key => $"[t].[{key.Name}] = @{key.Name}{index}")))));
            query.Append(" );");

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = query.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                foreach (var parameter in translationEntity.KeysFromSourceEntity
                    .Select(property => (Name: property.Value, Value: entity.GetType().GetProperty(property.Key).GetValue(entity))))
                {
                    command.AddParameterWithValue(parameter.Name, parameter.Value);
                }

                foreach (var parameter in translationEntities.Select((translation, entityIndex) => translationEntity.KeysFromLanguageEntity
                    .Select((key, keyIndex) => (Name: $"{key.Name}{entityIndex}", Value: translation.LanguageKey[keyIndex])))
                    .SelectMany(tuple => tuple))
                {
                    command.AddParameterWithValue(parameter.Name, parameter.Value);
                }

                context.Database.OpenConnection();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return SqlDataReaderToExpando(reader);
                    }
                }
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static IDictionary<string, object> SqlDataReaderToExpando(DbDataReader reader)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                expandoObject.Add(reader.GetName(i), reader[i]);
            }

            return expandoObject;
        }
    }
}
