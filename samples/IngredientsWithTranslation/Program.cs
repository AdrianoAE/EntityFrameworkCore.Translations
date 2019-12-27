using AdrianoAE.EntityFrameworkCore.Translations.Extensions;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using AdrianoAE.EntityFrameworkCore.Translations.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■ Domain Layer ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public class Ingredient
        {
            public int Id { get; internal set; } //internal set only to help with DataSeeding
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

        public class IngredientContext : DbContext
        {
            public DbSet<Ingredient> Ingredients { get; set; }
            public DbSet<IngredientTranslation> IngredientsTranslations { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TranslatedIngredientsSampleDB;ConnectRetryCount=0");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //Non existent Language table with (int)LanguageId as Foreign Key
                modelBuilder.ApplyTranslationsConfigurations(typeof(int), "LanguageId");

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
                    new { IngredientId = 3, LanguageId = Portuguese, Name = "Maça Podre" });
                #endregion
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
            await context.SaveChangesWithTranslationsAsync(DefaultLanguage);

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

            Console.WriteLine($"\tOriginal\tId: {rice.Id}\tName: {rice.Name}");

            rice.SetName("Bowl of Rice");
            context.Update(rice); //This is required because EFCore can't track the translated properties
            await context.SaveChangesWithTranslationsAsync(DefaultLanguage);

            #region Console Output
            var riceQuery = await context.Ingredients
                .AsNoTracking()
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 2);

            Console.WriteLine($"\tModified\tId: {riceQuery.Id}\tName: {riceQuery.Name}");
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            #region --- Add/Update translations
            var apple = await context.Ingredients
                .WithLanguage(DefaultLanguage)
                .WithFallback(DefaultLanguage)
                .FirstOrDefaultAsync(i => i.Id == 3);

            #region Console Output
            Console.WriteLine($"Original translations:");
            var appleTranslations = await context.IngredientsTranslations.AsNoTracking().Where(i => EF.Property<int>(i, "IngredientId") == 3).ToListAsync();
            foreach (var item in appleTranslations)
            {
                Console.WriteLine($"\tId: {apple.Id}\tName: {item.Name}");
            }
            #endregion

            var appleEnglish = new Ingredient("Apple");
            var applePortuguese = new Ingredient("Maça");
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
            Console.WriteLine($"\nAdded/Updated translations:");
            appleTranslations = await context.IngredientsTranslations.AsNoTracking().Where(i => EF.Property<int>(i, "IngredientId") == 3).ToListAsync();
            foreach (var item in appleTranslations)
            {
                Console.WriteLine($"\tId: {apple.Id}\tName: {item.Name}");
            }
            #endregion
            #endregion

            Console.WriteLine("\n■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

            Console.WriteLine($"(Query) Final Values:");
            await PrintAll(context);

            context.Database.EnsureDeleted();
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
