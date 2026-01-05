using Microsoft.EntityFrameworkCore;
using CV_siten.Models;

namespace CV_siten.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Projekt> Projekt { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Meddelande> Meddelanden { get; set; }
        public DbSet<CV> CVer { get; set; }

        // Lägg till denna metod här under:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Här skapar vi testpersonen som följer med i koden
            modelBuilder.Entity<Person>().HasData(new Person
            {
                Id = 1,
                Fornamn = "Oscar",
                Efternamn = "Test",
                Email = "test@test.se",
                Losenord = "123",
                Yrkestitel = "Systemutvecklare",
                Beskrivning = "Detta är en testprofil skapad via kod.",
                Aktivtkonto = true,
                Telefonnummer = 0701234567,
                BildUrl = ""
            });
        }
    }
}