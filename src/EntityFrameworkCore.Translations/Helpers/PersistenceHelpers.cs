using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace AdrianoAE.EntityFrameworkCore.Translations.Helpers
{
    public static class PersistenceHelpers
    {
        internal static void ValidateLanguageKeys(IEnumerable<KeyConfiguration> keys, object[] desiredParameters, object[] defaultParameters)
        {
            int parameterPosition = 0;
            foreach (var property in keys)
            {
                if (desiredParameters[parameterPosition].GetType() != defaultParameters[parameterPosition].GetType() ||
                    property.Type != desiredParameters[parameterPosition].GetType() ||
                    property.Type != defaultParameters[parameterPosition].GetType())
                {
                    throw new ArgumentException($"The following key does not match at index {parameterPosition}:\n" +
                        $"\tProperty: {property.Name} Type: {property.Type.Name}\n" +
                        $"\tDesired Type: {desiredParameters[parameterPosition].GetType().Name}\n" +
                        $"\tDefault Type: {defaultParameters[parameterPosition].GetType().Name}");
                }
                parameterPosition++;
            }
        }

        //═════════════════════════════════════════════════════════════════════════════════════════

        internal static void ValidateLanguageKeys(IEnumerable<KeyConfiguration> keys, object[] parameters)
            => ValidateLanguageKeys(keys, new List<object[]> { parameters });

        //═════════════════════════════════════════════════════════════════════════════════════════

        internal static void ValidateLanguageKeys(IEnumerable<KeyConfiguration> keys, IEnumerable<object[]> listOfParameters)
        {
            int parameterPosition;

            foreach (var parameters in listOfParameters)
            {
                parameterPosition = 0;
                foreach (var property in keys)
                {
                    if (property.Type != parameters[parameterPosition].GetType())
                    {
                        throw new ArgumentException($"The following key does not match at index {parameterPosition}:\n" +
                            $"\tProperty: {property.Name} Type: {property.Type.Name}\n" +
                            $"\tDesired Type: {parameters[parameterPosition].GetType().Name}");
                    }
                    parameterPosition++;
                }
            }
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static DbContext GetDbContext(IQueryable query)
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var queryCompiler = typeof(EntityQueryProvider).GetField("_queryCompiler", bindingFlags).GetValue(query.Provider);
            var queryContextFactory = queryCompiler.GetType().GetField("_queryContextFactory", bindingFlags).GetValue(queryCompiler);

            var dependencies = typeof(RelationalQueryContextFactory).GetField("_dependencies", bindingFlags).GetValue(queryContextFactory);
            var queryContextDependencies = typeof(DbContext).Assembly.GetType(typeof(QueryContextDependencies).FullName);
            var stateManagerProperty = queryContextDependencies.GetProperty("StateManager", bindingFlags | BindingFlags.Public).GetValue(dependencies);
            var stateManager = (IStateManager)stateManagerProperty;

#pragma warning disable EF1001 // Internal EF Core API usage.
            return stateManager.Context;
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        internal static void AddParameterWithValue(this DbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);
        }
    }
}
