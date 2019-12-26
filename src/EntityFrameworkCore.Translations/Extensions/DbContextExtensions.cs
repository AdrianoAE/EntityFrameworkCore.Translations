using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> SaveChangesWithTranslations([NotNull] this DbContext context, params object[] _languageKey)
        {
            int parameterPosition = 0;

            var addedEntries = context.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added
                    && TranslationConfiguration.TranslationEntities.ContainsKey(entry.Entity.GetType().FullName))
                .ToList();

            var modifiedEntries = context.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Modified
                    && TranslationConfiguration.TranslationEntities.ContainsKey(entry.Entity.GetType().FullName))
                .ToList();

            var result = await context.SaveChangesAsync();

            foreach (var entry in addedEntries)
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, _languageKey);

                var translation = Activator.CreateInstance(translationEntity.Type);

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    translationEntity.Type.GetProperty(property.Name)?.SetValue(translation, property.GetValue(entry.Entity), null);
                }

                parameterPosition = 0;
                foreach (var property in translationEntity.KeysFromLanguageEntity)
                {
                    context.Entry(translation).Property(property.Name).CurrentValue = _languageKey[parameterPosition];
                    parameterPosition++;
                }

                foreach (var property in translationEntity.KeysFromSourceEntity)
                {
                    context.Entry(translation).Property(property.Value).CurrentValue = entry.Entity.GetType().GetProperty(property.Key).GetValue(entry.Entity);
                }

                context.Entry(translation).State = EntityState.Added;
            }

            foreach (var entry in modifiedEntries)
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, _languageKey);

                var translatedEntry = context.ChangeTracker.Entries().Where(e =>
                {
                    if (e.Entity.GetType() != translationEntity.Type)
                    {
                        return false;
                    }

                    parameterPosition = 0;
                    foreach (var property in translationEntity.KeysFromLanguageEntity)
                    {
                        if (!e.Property(property.Name).CurrentValue.Equals(_languageKey[parameterPosition]))
                        {
                            return false;
                        }
                    }

                    foreach (var property in translationEntity.KeysFromSourceEntity)
                    {
                        if (!e.Property(property.Value).CurrentValue.Equals(entry.Property(property.Key).CurrentValue))
                        {
                            return false;
                        }
                    }

                    return true;
                }).SingleOrDefault();

                if (translatedEntry == null)
                {
                    throw new ArgumentNullException($"Could not find the translated entry for the entry {entry.Entity.GetType().Name}");
                }

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    if (translationEntity.Type.GetProperty(property.Name) != null)
                    {
                        context.Entry(translatedEntry.Entity).Property(property.Name).CurrentValue = property.GetValue(entry.Entity);
                    }
                }
            }

            return result += await context.SaveChangesAsync();
        }
    }
}
