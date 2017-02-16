using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using System;
using System.Linq;

namespace MvcMovie.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any movies.
                if (context.Movie.Any())
                {
                    return;   // DB has been seeded
                }

                context.Movie.AddRange(
                     new Movie
                     {
                         Title = "When Harry Met Sally",
                         ReleaseDate = "1989-1-11",
                         Genre = "Romantic Comedy",
                         Price = 7.99M,
                         Rating = "R"
                     },

                     new Movie
                     {
                         Title = "Ghostbusters ",
                         ReleaseDate = "1989-1-11",
                         Genre = "Comedy",
                         Price = 8.99M,
                         Rating = "PG-13"
                     },

                     new Movie
                     {
                         Title = "Ghostbusters 2",
                         ReleaseDate = "1989-1-11",
                         Genre = "Comedy",
                         Price = 9.99M,
                         Rating = "PG-13"
                     },

                   new Movie
                   {
                       Title = "Rio Bravo",
                       ReleaseDate = "1989-1-11",
                       Genre = "Western",
                       Price = 3.99M,
                       Rating = "R"
                   }
                );
                context.SaveChanges();
            }
        }
    }
}
