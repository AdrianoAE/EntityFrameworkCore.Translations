using AdrianoAE.EntityFrameworkCore.Translations;
using AdrianoAE.EntityFrameworkCore.Translations.Extensions;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IngredientsWithTranslation
{
    public class Program
    {
        public const int English = 1;
        public const int Portuguese = 2;
        public const int German = 3;
        public const int French = 4;
        public const int DefaultLanguage = English;

        public static IReadOnlyDictionary<string, object> OnSoftDeleteSetPropertyValue = new Dictionary<string, object>()
        {
            { "IsDeleted", 1}
        };

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■ Domain Layer ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public class Ingredient
        {
            public int Id { get; internal set; } //internal set for DataSeeding
            public string Name { get; private set; }

            private Ingredient() { } //Required by EF

            public Ingredient(string name)
                => SetName(name);

            public void SetName(string name)
                => Name = name;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■ Persistence Layer ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public class IngredientTranslation : ITranslation<Ingredient>
        {
            public string Name { get; set; }
        }

        #region --- Soft Delete Shadow Property Configurations
        public abstract class AuditableEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
              where TEntity : class
        {
            public virtual void Configure(EntityTypeBuilder<TEntity> builderConfiguration)
            {
                builderConfiguration.Property<bool>("IsDeleted")
                    .HasDefaultValue(false)
                    .IsRequired();

                builderConfiguration.HasQueryFilter(b => EF.Property<bool>(b, "IsDeleted") == false);
            }
        }

        public class IngredientEntityConfiguration : AuditableEntityTypeConfiguration<Ingredient>
        {
            public override void Configure(EntityTypeBuilder<Ingredient> categoryConfiguration)
                => base.Configure(categoryConfiguration);
        }

        public class IngredientTranslationEntityConfiguration : AuditableEntityTypeConfiguration<IngredientTranslation>
        {
            public override void Configure(EntityTypeBuilder<IngredientTranslation> categoryConfiguration)
                => base.Configure(categoryConfiguration);
        }
        #endregion

        public class IngredientContext : DbContext
        {
            public DbSet<Ingredient> Ingredients { get; set; }
            public DbSet<IngredientTranslation> IngredientsTranslations { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TranslatedIngredientsSampleDB;ConnectRetryCount=0");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                TranslationConfiguration.SetDeleteBehavior(DeleteBehavior.Cascade, true, OnSoftDeleteSetPropertyValue);

                modelBuilder.ApplyConfigurationsFromAssembly(typeof(IngredientContext).Assembly)
                    //Non existent Language table with (int)LanguageId as Foreign Key
                    //Check the overloads for more options
                    .ApplyTranslationsConfigurations(typeof(int), "LanguageId");

                #region Seeding
                modelBuilder.Entity<Ingredient>().HasData(
                    new Ingredient("") { Id = 1 },
                    new Ingredient("") { Id = 2 },
                    new Ingredient("") { Id = 3 });

                modelBuilder.Entity<IngredientTranslation>().HasData(
                    new { IngredientId = 1, LanguageId = English, Name = "Potato" },
                    new { IngredientId = 1, LanguageId = Portuguese, Name = "Batata" },
                    new { IngredientId = 2, LanguageId = English, Name = "Rice" },
                    new { IngredientId = 3, LanguageId = English, Name = "Rotten Apple" },
                    new { IngredientId = 3, LanguageId = Portuguese, Name = "Maçã Podre" });
                #endregion
            }

            public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                UpdateSoftDeleteStatuses();
                return await base.SaveChangesAsync(cancellationToken);
            }

            private void UpdateSoftDeleteStatuses()
            {
                foreach (var entry in ChangeTracker.Entries().Where(e => e.Properties.Any(p => p.Metadata.Name == "IsDeleted") && e.State == EntityState.Deleted))
                {
                    entry.State = EntityState.Modified;
                    entry.CurrentValues["IsDeleted"] = true;
                }
            }
        }

        //█████████████████████████████████████████████████████████████████████████████████████████

        public static async Task Main()
        {
            using var context = new IngredientContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            Console.WriteLine($"(Query) Initial values:");
            await PrintAll(context);

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            #region --- Insert - Intended to be used with the default language
            Console.WriteLine($"Insert:");

            var noodle = new Ingredient("Noodle");
            await context.Ingredients.AddAsync(noodle);
            await context.SaveChangesWithTranslationsAsync(default, DefaultLanguage);

            #region Console Output
            var noodleResult = await context.Ingredients
                .AsNoTracking()
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 4);

            Console.WriteLine($"\tId: {noodleResult.Id}\tName: {noodleResult.Name}");
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            #region --- Update - Intended to be used with the default language
            Console.WriteLine($"Update:");

            var rice = await context.Ingredients
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 2);

            Console.WriteLine($"    Original:\n\tId: {rice.Id}\tName: {rice.Name}");

            rice.SetName("Bowl of Rice");
            context.Update(rice); //This is required because EFCore can't track the translated properties
            await context.SaveChangesWithTranslationsAsync(default, DefaultLanguage);

            #region Console Output
            var riceQuery = await context.Ingredients
                .AsNoTracking()
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 2);

            Console.WriteLine($"    Modified:\n\tId: {riceQuery.Id}\tName: {riceQuery.Name}");
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            #region --- Add/Update translations
            Console.WriteLine($"Add/Update translations:");

            var apple = await context.Ingredients
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 3);

            #region Console Output
            Console.WriteLine($"    Original:");
            var appleTranslations = await context.IngredientsTranslations.AsNoTracking().Where(i => EF.Property<int>(i, "IngredientId") == 3).ToListAsync();
            foreach (var item in appleTranslations)
            {
                Console.WriteLine($"\tId: {apple.Id}\tName: {item.Name}");
            }
            #endregion

            var appleEnglish = new Ingredient("Apple");
            var applePortuguese = new Ingredient("Maçã");
            var appleGerman = new Ingredient("Apfel");
            var appleFrench = new Ingredient("Pomme");

            //Here to showcase, always use the Range one for multiple
            context.Ingredients.UpsertTranslation(apple, appleEnglish, English); //Update

            context.Ingredients.UpsertTranslationRange(apple,
                applePortuguese.TranslationOf(Portuguese),  //Update
                appleGerman.TranslationOf(German),          //Add
                appleFrench.TranslationOf(French));         //Add

            await context.SaveChangesAsync();

            #region Console Output
            Console.WriteLine($"\n    Added/Updated:");
            appleTranslations = await context.IngredientsTranslations.AsNoTracking().Where(i => EF.Property<int>(i, "IngredientId") == 3).ToListAsync();
            foreach (var item in appleTranslations)
            {
                Console.WriteLine($"\tId: {apple.Id}\tName: {item.Name}");
            }
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            #region --- Soft Delete enabled for translation tables
            Console.WriteLine($"Potato soft deleted.");

            var ingredientToDelete = await context.Ingredients.FirstOrDefaultAsync(i => i.Id == 1);
            context.Ingredients.Remove(ingredientToDelete);
            await context.SaveChangesWithTranslationsAsync();

            #region Console Output
            foreach (var property in OnSoftDeleteSetPropertyValue)
            {
                Console.WriteLine($"\tProperty '{property.Key}' set to '{property.Value}'");
            }
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            Console.WriteLine($"(Query) Final Values:");
            await PrintAll(context);


            var translationEntity = TranslationConfiguration.TranslationEntities[typeof(Ingredient).FullName];

            var selectedIngredient = await context.Ingredients
                .FirstOrDefaultAsync(ingredient => ingredient.Id == 3);
            var selectedIngredientTranslations = await context.IngredientsTranslations
                //The predicate must be built at runtime with KeysFromSource
                .Where(translation => EF.Property<int>(translation, "IngredientId") == 3)
                .ToListAsync();

            IDictionary<string, object> selectedIngredientMapping = new ExpandoObject();

            foreach (var property in typeof(Ingredient).GetProperties())
            {
                if (typeof(IngredientTranslation).GetProperty(property.Name) != null)
                {
                    var translations = new Dictionary<object, object>();

                    foreach (var translation in selectedIngredientTranslations)
                    {
                        var languageKey = new Dictionary<string, object>();

                        foreach (var languageProperty in typeof(IngredientTranslation)
                            .GetProperties()
                            .Where(languageProperty => translationEntity.KeysFromLanguage
                                .Select(key => key.Name)
                                .Contains(languageProperty.Name)))
                        {
                            languageKey.Add(languageProperty.Name, languageProperty.GetValue(translation));
                        }

                        if (languageKey.Count == 1)
                        {
                            translations.Add(languageKey.First().Value, translation.Name);
                        }
                        else
                        {
                            translations.Add(languageKey, translation.Name);
                        }
                    }

                    selectedIngredientMapping.Add($"{property.Name}Translations", translations);
                }
                else
                {
                    selectedIngredientMapping.Add(property.Name, property.GetValue(selectedIngredient));
                }
            }

            //-----------------------------------------------

            var ingredients = await context.Ingredients
                .Where(ingredient => ingredient.Id == 2 || ingredient.Id == 4)
                .ToListAsync();
            var ingredientsTranslations = await context.IngredientsTranslations
                //The predicate must be built at runtime with KeysFromSource
                .Where(translation => ingredients
                    .Select(ingredient => ingredient.Id)
                    .Contains(EF.Property<int>(translation, "IngredientId")))
                .ToListAsync();

            var translatedIngredients = new List<dynamic>();

            foreach (var ingredient in ingredients)
            {
                IDictionary<string, object> ingredientMapping = new ExpandoObject();

                foreach (var property in typeof(Ingredient).GetProperties())
                {
                    if (typeof(IngredientTranslation).GetProperty(property.Name) != null)
                    {
                        var translations = new Dictionary<object, object>();

                        foreach (var translation in ingredientsTranslations
                            //The predicate must be built at runtime with KeysFromSource
                            .Where(translation => context.Entry(translation).Property<int>("IngredientId").CurrentValue == ingredient.Id))
                        {
                            var languageKey = new Dictionary<string, object>();

                            foreach (var languageProperty in typeof(IngredientTranslation)
                                .GetProperties()
                                .Where(p => translationEntity.KeysFromLanguage
                                    .Select(x => x.Name)
                                    .Contains(p.Name)))
                            {
                                languageKey.Add(languageProperty.Name, context.Entry(languageProperty).Property<object>(languageProperty.Name).CurrentValue);
                            }

                            if (languageKey.Count == 1)
                            {
                                translations.Add(languageKey.First().Value, translation.Name);
                            }
                            else
                            {
                                translations.Add(languageKey, translation.Name);
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

            //----------------------

            var selectedIngredientMappingSerialized = JsonConvert.SerializeObject(selectedIngredientMapping);
            var translatedIngredientsSerialized = JsonConvert.SerializeObject(translatedIngredients);
        }

        private static async Task PrintAll(IngredientContext context)
        {
            var ingredients = await context.Ingredients
                .WithLanguage(Portuguese)
                .WithFallback(DefaultLanguage)
                .ToListAsync();

            foreach (var ingredient in ingredients)
            {
                Console.WriteLine($"\tId: {ingredient.Id}\tName: {ingredient.Name}");
            }
        }
    }
}
