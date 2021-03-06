﻿using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdrianoAE.EntityFrameworkCore.Translations.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Type type, string foreignKeyName)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(type, foreignKeyName));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, string schema, Type type, string foreignKeyName)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(schema, type, foreignKeyName));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, IEnumerable<KeyConfiguration> primaryKey)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(primaryKey));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, string schema, IEnumerable<KeyConfiguration> primaryKey)
            => modelBuilder.ApplyTranslationsConfigurations(new LanguageTableConfiguration(schema, primaryKey));

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, LanguageTableConfiguration configuration)
        {
            TranslationConfiguration.LanguageTableConfiguration = configuration;

            return modelBuilder.Configure();
        }

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
            => modelBuilder.Configure(assembly.GetTypes().Where(t => t.IsClass && typeof(ILanguageEntity).IsAssignableFrom(t)).SingleOrDefault());

        //─────────────────────────────────────────────────────────────────────────────────────────

        public static ModelBuilder ApplyTranslationsConfigurations(this ModelBuilder modelBuilder, Type languageEntity)
            => modelBuilder.Configure(languageEntity);
    }
}
