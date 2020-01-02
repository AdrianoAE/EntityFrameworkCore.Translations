using System;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations.Helpers
{
    internal static class GenericTypeHelper
    {
        internal static string GetGenericTypeName(this Type type)
        {
            var typeName = string.Empty;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        internal static string GetGenericTypeName(this object @object) 
            => @object.GetType().GetGenericTypeName();
    }
}
