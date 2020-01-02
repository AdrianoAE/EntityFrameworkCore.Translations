using AdrianoAE.EntityFrameworkCore.Translations.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> SaveChangesWithTranslationsAsync([NotNull] this DbContext context, CancellationToken cancellationToken = default, params object[] languageKey)
        {
            bool hasActiveTransaction = context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = context.Database.CurrentTransaction
                ?? await context.Database.BeginTransactionAsync(TranslationConfiguration.IsolationLevel, cancellationToken);

            var entries = context.ChangeTracker.Entries()
                .Where(entry => TranslationConfiguration.TranslationEntities.ContainsKey(entry.Entity.GetType().FullName))
                .ToLookup(entry => entry.State);

            var result = await context.SaveChangesAsync(cancellationToken);

            foreach (var state in entries)
            {
                switch (state.Key)
                {
                    case EntityState.Added:
                        ConfigureAddedEntries(context, state, languageKey);
                        break;
                    case EntityState.Modified:
                        ConfigureModifiedEntries(context, state, languageKey);
                        break;
                    case EntityState.Deleted:
                        await ConfigureDeletedEntries(context, state, cancellationToken);
                        break;
                    default:
                        break;
                }
            }

            result += await context.SaveChangesAsync(cancellationToken);

            if (!hasActiveTransaction)
            {
                await CommitTransactionAsync(transaction, cancellationToken);
            }

            return result;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureAddedEntries(DbContext context, IGrouping<EntityState, EntityEntry> state, object[] languageKey)
        {
            int parameterPosition;

            foreach (var entry in state)
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, languageKey);

                var translation = Activator.CreateInstance(translationEntity.Type);

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    translationEntity.Type.GetProperty(property.Name)?.SetValue(translation, property.GetValue(entry.Entity));
                }

                parameterPosition = 0;
                foreach (var property in translationEntity.KeysFromLanguageEntity)
                {
                    context.Entry(translation).Property(property.Name).CurrentValue = languageKey[parameterPosition];
                    parameterPosition++;
                }

                foreach (var property in translationEntity.KeysFromSourceEntity)
                {
                    context.Entry(translation).Property(property.Value).CurrentValue = entry.Entity.GetType().GetProperty(property.Key).GetValue(entry.Entity);
                }

                context.Entry(translation).State = EntityState.Added;
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void ConfigureModifiedEntries(DbContext context, IGrouping<EntityState, EntityEntry> state, object[] languageKey)
        {
            int parameterPosition;

            foreach (var entry in state)
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                PersistenceHelpers.ValidateLanguageKeys(translationEntity.KeysFromLanguageEntity, languageKey);

                var translatedEntry = context.ChangeTracker.Entries().Where(e =>
                {
                    if (e.Entity.GetType() != translationEntity.Type)
                    {
                        return false;
                    }

                    parameterPosition = 0;
                    foreach (var property in translationEntity.KeysFromLanguageEntity)
                    {
                        if (!e.Property(property.Name).CurrentValue.Equals(languageKey[parameterPosition]))
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
                    throw new ArgumentNullException($"Could not find the translated entry for the {entry.Entity.GetType().Name} entry");
                }

                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    if (translationEntity.Type.GetProperty(property.Name) != null)
                    {
                        context.Entry(translatedEntry.Entity).Property(property.Name).CurrentValue = property.GetValue(entry.Entity);
                    }
                }
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static async Task ConfigureDeletedEntries(DbContext context, IGrouping<EntityState, EntityEntry> state, CancellationToken cancellationToken = default)
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            var query = new StringBuilder();
            int parameterPosition = 0;

            foreach (var entry in state)
            {
                var translationEntity = TranslationConfiguration.TranslationEntities[entry.Entity.GetType().FullName];

                if (translationEntity.SoftDelete && translationEntity.DeleteBehavior == DeleteBehavior.Cascade)
                {
                    if (translationEntity.OnSoftDeleteSetPropertyValue == null)
                    {
                        throw new ArgumentNullException(nameof(translationEntity.OnSoftDeleteSetPropertyValue), "Soft delete requires configuration of the properties and the desired value to be set on delete." +
                            " Configure it for all entities using TranslationConfiguration.SetDeleteBehavior(DeleteBehavior.Cascade, true, new Dictionary<string, object> { ... })" +
                            " and/or per entity using entityTypeBuilder.TranslationDeleteBehavior(DeleteBehavior.Cascade, true, new Dictionary<string, object> { ... })");
                    }

                    var schema = !string.IsNullOrWhiteSpace(translationEntity.Schema) ? $"[{translationEntity.Schema}]." : string.Empty;

                    query.Append($"UPDATE {schema}[{translationEntity.TableName}] SET ");
                    query.Append(string.Join(", ", translationEntity.OnSoftDeleteSetPropertyValue.Select(key => $"[{key.Key}] = @{key.Key}{parameterPosition}")));
                    query.Append(" WHERE ");
                    query.Append(string.Join(" AND ", translationEntity.KeysFromSourceEntity
                        .Select(property => $"[{property.Value}] = @{property.Value}{parameterPosition}")));
                    query.Append(";");

                    foreach (var property in translationEntity.OnSoftDeleteSetPropertyValue)
                    {
                        command.AddParameterWithValue($"{property.Key}{parameterPosition}", property.Value);
                    }

                    foreach (var parameter in translationEntity.KeysFromSourceEntity
                        .Select(property => (Name: property.Value, Value: entry.Entity.GetType().GetProperty(property.Key).GetValue(entry.Entity))))
                    {
                        command.AddParameterWithValue($"{parameter.Name}{parameterPosition}", parameter.Value);
                    }

                    entry.State = EntityState.Unchanged;
                    parameterPosition++;
                }
            }

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = query.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

            if (!string.IsNullOrWhiteSpace(query.ToString()))
            {
                command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();

                await context.Database.OpenConnectionAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                RollbackTransaction(transaction);
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        private static void RollbackTransaction(IDbContextTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            finally
            {
                transaction.Dispose();
            }
        }
    }
}
