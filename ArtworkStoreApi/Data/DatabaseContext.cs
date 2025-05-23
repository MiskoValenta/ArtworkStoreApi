using ArtworkStoreApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ArtworkStoreApi.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            // Jakože nic no...
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Genre>().HasData(
                new { Id = 1, Name = "Abstrakce" },
                new { Id = 2, Name = "Design Decor Art" },
                new { Id = 3, Name = "Krajiny" },
                new { Id = 4, Name = "Motiv" },
                new { Id = 5, Name = "Zvířata" }
                );
        }
    }
}
