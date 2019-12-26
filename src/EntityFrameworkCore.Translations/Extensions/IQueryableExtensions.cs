using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class IQueryableExtensions
    {
        public static ITranslationQueryInitialized<TEntity> WithLanguage<TEntity>([NotNull] this IQueryable<TEntity> source, params object[] languageKey)
            where TEntity : class
            => TranslationQuery<TEntity>.Initialize(source, languageKey);

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public static void UpsertTranslation<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity entity, TEntity translation, params object[] languageKey)
            where TEntity : class
            => UpsertTranslationRange(source, entity, new Translation<TEntity>(translation, languageKey));

        //═════════════════════════════════════════════════════════════════════════════════════════

        private static MethodInfo configureEntityMethod => typeof(IQueryableExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(t => t.IsGenericMethod && t.Name == nameof(teste));


        public static void UpsertTranslationRange<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity entity, params Translation<TEntity>[] translationEntities)
            where TEntity : class
        {
            int parameterPosition;
            var context = PersistenceHelpers.GetDbContext(source);
            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];

            PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, translationEntities.SelectMany(t => t.LanguageKey).ToArray());

            var schema = !string.IsNullOrWhiteSpace(translationEntity.Schema) ? $"[{translationEntity.Schema}]." : string.Empty;

            var method = configureEntityMethod.MakeGenericMethod(typeof(TEntity), translationEntity.Type);
            var existingTranslations = (IEnumerable<IDictionary<string, object>>)method.Invoke(null, new object[] { context, entity, translationEntity, schema, translationEntities });

            //Duplicado refactor
            foreach (var entry in translationEntities)
            {
                var translation = Activator.CreateInstance(translationEntity.Type);

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    translationEntity.Type.GetProperty(property.Name)?.SetValue(translation, property.GetValue(entry.Entity), null);
                }

                parameterPosition = 0;
                foreach (var property in translationEntity.KeysFromLanguageEntity)
                {
                    context.Entry(translation).Property(property.Name).CurrentValue = entry.LanguageKey[parameterPosition];
                    parameterPosition++;
                }

                foreach (var property in translationEntity.KeysFromSourceEntity)
                {
                    context.Entry(translation).Property(property.Value).CurrentValue = entity.GetType().GetProperty(property.Key).GetValue(entity);
                }

                //Build predicate that tries to find object with all propertyName - Value of translation in existingTranslations
                foreach (var item in translation.GetType().GetProperties())
                {

                }

                context.Entry(translation).State = EntityState.Added;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static IEnumerable<IDictionary<string, object>> teste<TEntity, TTranslatedEntity>(this DbContext context, TEntity entity,
            TranslationEntity translationEntity, string schema, Translation<TEntity>[] translationEntities)
            where TEntity : class
            where TTranslatedEntity : class
        {
            var query = new StringBuilder();
            query.Append("SELECT ");
            query.Append(string.Join(" ,", context.Model.FindEntityType(translationEntity.Type).GetProperties().Select(x => $"[t].[{x.GetColumnName()}]")));
            query.Append($" FROM {schema}[{translationEntity.TableName}] AS [t]");
            query.Append(" WHERE ");
            query.Append(string.Join(" ,", translationEntity.KeysFromSourceEntity
                .Select(property => $"[t].[{property.Value}] = {entity.GetType().GetProperty(property.Key).GetValue(entity)}")));
            query.Append(" AND (");
            query.Append(string.Join(" OR ", translationEntities
                .Select(translation => string.Join(" ,", translationEntity.KeysFromLanguageEntity
                    .Select((key, index) => $"[t].[{key.Name}] = {translation.LanguageKey[index]}")))));
            query.Append(" );");

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                //Solve problems with SQL Injection
                command.CommandText = query.ToString();
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

        private static IDictionary<string, object> SqlDataReaderToExpando(DbDataReader reader)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
                expandoObject.Add(reader.GetName(i), reader[i]);

            return expandoObject;
        }
    }
}
