using AdrianoAE.EntityFrameworkCore.Translations.Extensions;
using AdrianoAE.EntityFrameworkCore.Translations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IngredientsWithTranslation
{
    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■ Domain Layer ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    public class Ingredient
    {
        public int Id { get; internal set; } //internal set only to help with DataSeeding
        public string Name { get; private set; }

        private Ingredient() { } //Required by EF

        public Ingredient(string name)
        {
            Name = name;
        }
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
            => optionsBuilder.EnableSensitiveDataLogging().UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Non existent Language table with (int)LanguageId as Foreign Key
            modelBuilder.ApplyTranslationsConfigurations(typeof(int), "LanguageId");

            #region Seeding
            modelBuilder.Entity<Ingredient>().HasData(new Ingredient("Potato") { Id = 1 });
            modelBuilder.Entity<Ingredient>().HasData(new Ingredient("Rice") { Id = 2 });

            modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 1, LanguageId = 1, Name = "Potato" });
            modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 1, LanguageId = 2, Name = "Batata" });
            modelBuilder.Entity<IngredientTranslation>().HasData(new { IngredientId = 2, LanguageId = 1, Name = "Rice" });
            #endregion
        }
    }

    //█████████████████████████████████████████████████████████████████████████████████████████████

    public class Program
    {
        public static async Task Main()
        {
            int english = 1;
            int portuguese = 2;

            using (var context = new IngredientContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //Queries
                var ingredients = await context.Ingredients
                    .WithLanguage(portuguese)
                    .WithFallback(english)
                    .ToListAsync();

                Console.WriteLine($"Query:");
                foreach (var ingredient in ingredients)
                {
                    Console.WriteLine($"\tId: {ingredient.Id}\tName: {ingredient.Name}");
                }

                //Insert - Intended to be used with the default language
                var noodle = new Ingredient("Noodle");
                await context.Ingredients.AddAsync(noodle);
                await context.SaveChangesWithTranslations(english);

                var noodleResult = await context.Ingredients
                    .WithLanguage(1)
                    .WithFallback(1)
                    .FirstOrDefaultAsync(i => i.Id == 3);

                Console.WriteLine($"\nInsert:");
                Console.WriteLine($"\tId: {noodleResult.Id}\tName: {noodleResult.Name}");

                //TODO: Update - Intended to be used with the default language

                //TODO: Allow to add translations to other languages

                context.Database.EnsureDeleted();
            }
        }
    }
}
