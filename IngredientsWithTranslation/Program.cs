using AdrianoAE.EntityFrameworkCore.Translations.Extensions;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IngredientsWithTranslation
{
    public class Program
    {
        public const int ENGLISH = 1;
        public const int PORTUGUESE = 2;

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
                    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //Non existent Language table with (int)LanguageId as Foreign Key
                modelBuilder.ApplyTranslationsConfigurations(typeof(int), "LanguageId");

                #region Seeding
                modelBuilder.Entity<Ingredient>().HasData(new Ingredient("Potato") { Id = 1 });
                modelBuilder.Entity<Ingredient>().HasData(new Ingredient("Rice") { Id = 2 });

                modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 1, LanguageId = ENGLISH, Name = "Potato" });
                modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 1, LanguageId = PORTUGUESE, Name = "Batata" });
                modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 2, LanguageId = ENGLISH, Name = "Rice" });
                #endregion
            }
        }

        //█████████████████████████████████████████████████████████████████████████████████████████

        public static async Task Main()
        {
            using var context = new IngredientContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            #region --- Query
            Console.WriteLine($"Query:");

            var ingredients = await context.Ingredients
                .WithLanguage(PORTUGUESE)
                .WithFallback(ENGLISH)
                .ToListAsync();

            foreach (var ingredient in ingredients)
            {
                Console.WriteLine($"\tId: {ingredient.Id}\tName: {ingredient.Name}");
            }
            #endregion

            //Intended to be used with the default language
            #region --- Insert
            Console.WriteLine($"\nInsert:");

            var noodle = new Ingredient("Noodle");
            await context.Ingredients.AddAsync(noodle);
            await context.SaveChangesWithTranslationsAsync(ENGLISH);

            var noodleResult = await context.Ingredients
                .WithLanguage(ENGLISH)
                .WithFallback(ENGLISH)
                .FirstOrDefaultAsync(i => i.Id == 3);

            Console.WriteLine($"\tId: {noodleResult.Id}\tName: {noodleResult.Name}");
            #endregion

            //Intended to be used with the default language
            #region --- Update
            Console.WriteLine($"\nUpdate:");

            var rice = await context.Ingredients
                .WithLanguage(ENGLISH)
                .WithFallback(ENGLISH)
                .FirstOrDefaultAsync(i => i.Id == 2);

            Console.WriteLine($"\tOriginal\tId: {rice.Id}\tName: {rice.Name}");

            rice.SetName("Bowl of Rice");
            context.Update(rice); //This is required because EFCore can't track the translated properties
            await context.SaveChangesWithTranslationsAsync(ENGLISH);

            var riceQuery = await context.Ingredients
                .WithLanguage(ENGLISH)
                .WithFallback(ENGLISH)
                .FirstOrDefaultAsync(i => i.Id == 2);

            Console.WriteLine($"\tModified\tId: {riceQuery.Id}\tName: {riceQuery.Name}");
            #endregion

            //TODO: Allow to add/update translations

            context.Database.EnsureDeleted();
        }
    }
}
