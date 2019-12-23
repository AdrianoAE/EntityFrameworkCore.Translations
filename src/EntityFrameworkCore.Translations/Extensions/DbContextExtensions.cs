using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> SaveChangesWithTranslations([NotNull] this DbContext context)
        {
            var _desiredParameters = new object[] { 1 };
            var result = await context.SaveChangesAsync();

            foreach (var entry in new List<EntityEntry>(context.ChangeTracker.Entries().Where(entry => TranslationConfiguration.TranslationEntities.ContainsKey(entry.Entity.GetType().FullName))))
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, _desiredParameters);

                var translation = Activator.CreateInstance(translationEntity.Type);
                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    translationEntity.Type.GetProperty(property.Name)?.SetValue(translation, property.GetValue(entry.Entity), null);
                }

                int parameterPosition = 0;
                foreach (var property in translationEntity.KeysFromLanguageEntity)
                {
                    context.Entry(translation).Property(property.Name).CurrentValue = _desiredParameters[parameterPosition];
                    parameterPosition++;
                }

                foreach (var property in translationEntity.KeysFromSourceEntity)
                {
                    context.Entry(translation).Property(property.Value).CurrentValue = entry.Entity.GetType().GetProperty(property.Key).GetValue(entry.Entity);
                }

                context.Entry(translation).State = EntityState.Added;
            }

            return result += await context.SaveChangesAsync();
        }
    }
}
