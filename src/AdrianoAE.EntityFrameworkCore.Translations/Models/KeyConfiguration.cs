using System;

namespace AdrianoAE.EntityFrameworkCore.Translations.Models
{
    public class KeyConfiguration
    {
        public readonly Type Type;
        public readonly string Name;

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public KeyConfiguration(Type type, string name)
        {
            Type = type;
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(Name));
        }
    }
}
