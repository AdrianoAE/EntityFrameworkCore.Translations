using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        public static  void UpsertTranslationRange<TEntity>([NotNull] this IQueryable<TEntity> source, TEntity sourceEntity, params Translation<TEntity>[] translationEntities)
            where TEntity : class
        {
            int parameterPosition;
            var context = PersistenceHelpers.GetDbContext(source);
            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];

            PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, translationEntities.SelectMany(t => t.LanguageKey).ToArray());

            var query = new StringBuilder();
            query.Append("SELECT ");
            query.Append(string.Join(" ,", translationEntity.KeysFromLanguageEntity.Select(x => $"[t].[{x.Name}]")));
            query.Append($" FROM {translationEntity.Schema}.[{translationEntity.TableName}] AS [t]");
            query.Append(" WHERE ");
            query.Append(string.Join(" ,", translationEntity.KeysFromSourceEntity
                .Select(property => $"[t].[{property.Value}] == {sourceEntity.GetType().GetProperty(property.Key).GetValue(sourceEntity)}")));
            query.Append(" AND (");
            query.Append(string.Join(" OR ", translationEntities
                .Select(translation => string.Join(" ,", translationEntity.KeysFromLanguageEntity
                    .Select(x => $"[t].[{x.Name}] == {translation.LanguageKey[0]}")))));
            query.Append(" );");

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
                    context.Entry(translation).Property(property.Value).CurrentValue = sourceEntity.GetType().GetProperty(property.Key).GetValue(sourceEntity);
                }

                context.Entry(translation).State = EntityState.Added;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static Func<object, bool> BuildPredicate(string name, object value)
            => entity => EF.Property<object>(entity, name) == value;
    }
}
