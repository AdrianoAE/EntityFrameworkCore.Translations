using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    internal static class TranslatedIQueryableBuilder
    {
        internal static IQueryable<TEntity> GetTranslatedQuery<TEntity>(this IQueryable<TEntity> query, object[] desiredLanguageKey, object[] defaultLanguageKey,
            Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            var context = PersistenceHelpers.GetDbContext(query);
            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];

            PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, desiredLanguageKey, defaultLanguageKey);

            var baseQuery = context.GetType()
                .GetMethod("Query")
                .MakeGenericMethod(translationEntity.Type);

            var defaultTranslationQuery = (IQueryable<object>)baseQuery.Invoke(context, null);
            var desiredTranslationQuery = (IQueryable<object>)baseQuery.Invoke(context, null);

            int parameterPosition = 0;
            foreach (var property in translationEntity.KeysFromLanguageEntity)
            {
                defaultTranslationQuery = defaultTranslationQuery.Where(BuildPredicate(property.Name, defaultLanguageKey[parameterPosition]));
                desiredTranslationQuery = desiredTranslationQuery.Where(BuildPredicate(property.Name, desiredLanguageKey[parameterPosition]));
                parameterPosition++;
            }

            var sourceKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Key);
            var relationshipKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Value);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

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

        internal static async Task<List<dynamic>> GetAllTranslationsQuery<TEntity>(this IQueryable<TEntity> query, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var context = PersistenceHelpers.GetDbContext(query);
            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(TEntity).FullName];

            var ingredients = await query.ToListAsync(cancellationToken);

            var sourceKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Key);
            var relationshipKeys = translationEntity.KeysFromSourceEntity.Select(key => key.Value);
                        
            var ingredientsTranslations = await query
                .Join(
                    context.GetType().GetMethod("Query").MakeGenericMethod(translationEntity.Type).Invoke(context, null) as IQueryable<object>,
                    GetKeys<TEntity>(sourceKeys),
                    GetKeys<object>(relationshipKeys),
                    (Entity, Translation) => Translation)
                .ToListAsync(cancellationToken);

            var translatedIngredients = new List<dynamic>();

            foreach (var ingredient in ingredients)
            {
                IDictionary<string, object> ingredientMapping = new ExpandoObject();

                foreach (var property in typeof(TEntity).GetProperties())
                {
                    if (translationEntity.Type.GetProperty(property.Name) != null)
                    {
                        var translations = new Dictionary<object, object>();

                        var currentIngredientTranslations = ingredientsTranslations.AsQueryable();
                        foreach (var keyFromSource in translationEntity.KeysFromSourceEntity)
                        {
                            currentIngredientTranslations = currentIngredientTranslations
                                .Where(translation => context
                                    .Entry(translation)
                                    .Property(keyFromSource.Value).CurrentValue
                                    .Equals(ingredient.GetType().GetProperty(keyFromSource.Key).GetValue(ingredient)));
                        }

                        foreach (var translation in currentIngredientTranslations)
                        {
                            var languageKey = new Dictionary<string, object>();

                            foreach (var languageProperty in translationEntity.KeysFromLanguageEntity)
                            {
                                languageKey.Add(languageProperty.Name, context.Entry(translation).Property(languageProperty.Name).CurrentValue);
                            }

                            if (languageKey.Count == 1)
                            {
                                translations.Add(languageKey.First().Value, translation.GetType().GetProperty(property.Name).GetValue(translation));
                            }
                            else
                            {
                                translations.Add(languageKey, translation.GetType().GetProperty(property.Name).GetValue(translation));
                            }
                        }

                        ingredientMapping.Add($"{property.Name}Translations", translations);
                    }
                    else
                    {
                        ingredientMapping.Add(property.Name, property.GetValue(ingredient));
                    }
                }

                translatedIngredients.Add(ingredientMapping);
            }
            
            return translatedIngredients;
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
                entityType.GetProperty(property.Name)?.SetValue(entity, property.GetValue(translation));
            }

            return entity;
        }
    }
}
