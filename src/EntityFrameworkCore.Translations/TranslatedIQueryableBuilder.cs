using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    internal static class TranslatedIQueryableBuilder
    {
        internal static IQueryable<TEntity> GetTranslatedQuery<TEntity>(this IQueryable<TEntity> query, object[] desiredParameters, object[] defaultParameters,
            Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var context = PersistenceHelpers.GetDbContext(query);

            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];

            PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, desiredParameters, defaultParameters);

            var baseQuery = context.GetType()
                .GetMethod("Query")
                .MakeGenericMethod(translationEntity.Type);

            var defaultTranslationQuery = (IQueryable<object>)baseQuery.Invoke(context, null);
            var desiredTranslationQuery = (IQueryable<object>)baseQuery.Invoke(context, null);

            int parameterPosition = 0;
            foreach (var property in translationEntity.KeysFromLanguageEntity)
            {
                defaultTranslationQuery = defaultTranslationQuery.Where(BuildPredicate(property.Name, defaultParameters[parameterPosition]));
                desiredTranslationQuery = desiredTranslationQuery.Where(BuildPredicate(property.Name, desiredParameters[parameterPosition]));
                parameterPosition++;
            }

            var sourceKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Key);
            var relationshipKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Value);

            var wanted = query
                //Fallback
                .Join(
                    defaultTranslationQuery,
                    GetKeys<TEntity>(sourceKeys),
                    GetKeys<object>(relationshipKeys),
                    (Entity, Translation) => new QueryJoinHelper<TEntity> { Entity = Entity, Translation = Translation })
                //Wanted
                .GroupJoin(
                    desiredTranslationQuery,
                    GetKeysFromGroup<QueryJoinHelper<TEntity>, TEntity>(sourceKeys),
                    GetKeys<object>(relationshipKeys),
                    (Base, Translation) => new { Base, Translation })
                .SelectMany(result => result.Translation.DefaultIfEmpty(),
                    (From, Translation) => From.Base.Entity.SetTranslatedProperties(Translation ?? From.Base.Translation));

            return wanted;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static Expression<Func<object, bool>> BuildPredicate(string name, object value)
            => entity => EF.Property<object>(entity, name) == value;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static Expression<Func<T, object>> GetKeys<T>(IEnumerable<string> keys)
            => keys.Count() switch
            {
                1 => entity => new
                {
                    K1 = EF.Property<object>(entity, keys.ElementAt(0))
                },
                2 => entity => new
                {
                    K1 = EF.Property<object>(entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity, keys.ElementAt(1))
                },
                3 => entity => new
                {
                    K1 = EF.Property<object>(entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity, keys.ElementAt(2))
                },
                4 => entity => new
                {
                    K1 = EF.Property<object>(entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity, keys.ElementAt(2)),
                    K4 = EF.Property<object>(entity, keys.ElementAt(3))
                },
                5 => entity => new
                {
                    K1 = EF.Property<object>(entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity, keys.ElementAt(2)),
                    K4 = EF.Property<object>(entity, keys.ElementAt(3)),
                    K5 = EF.Property<object>(entity, keys.ElementAt(4))
                },
                _ => throw new NotSupportedException($"{keys.Count()} keys provided. The maximum number of keys supported is 5.")
            };

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static Expression<Func<T, object>> GetKeysFromGroup<T, TEntity>(IEnumerable<string> keys)
            where T : QueryJoinHelper<TEntity>
            => keys.Count() switch
            {
                1 => entity => new
                {
                    K1 = EF.Property<object>(entity.Entity, keys.ElementAt(0))
                },
                2 => entity => new
                {
                    K1 = EF.Property<object>(entity.Entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity.Entity, keys.ElementAt(1))
                },
                3 => entity => new
                {
                    K1 = EF.Property<object>(entity.Entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity.Entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity.Entity, keys.ElementAt(2))
                },
                4 => entity => new
                {
                    K1 = EF.Property<object>(entity.Entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity.Entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity.Entity, keys.ElementAt(2)),
                    K4 = EF.Property<object>(entity.Entity, keys.ElementAt(3))
                },
                5 => entity => new
                {
                    K1 = EF.Property<object>(entity.Entity, keys.ElementAt(0)),
                    K2 = EF.Property<object>(entity.Entity, keys.ElementAt(1)),
                    K3 = EF.Property<object>(entity.Entity, keys.ElementAt(2)),
                    K4 = EF.Property<object>(entity.Entity, keys.ElementAt(3)),
                    K5 = EF.Property<object>(entity.Entity, keys.ElementAt(4))
                },
                _ => throw new NotSupportedException($"{keys.Count()} keys provided. The maximum number of keys supported is 5.")
            };

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static TEntity SetTranslatedProperties<TEntity, TSource>(this TEntity entity, TSource translation)
            where TEntity : class
        {
            if (translation == null)
            {
                return null;
            }

            var entityType = entity.GetType();

            foreach (var property in translation.GetType().GetProperties())
            {
                entityType.GetProperty(property.Name)?.SetValue(entity, property.GetValue(translation), null);
            }

            return entity;
        }
    }
}
