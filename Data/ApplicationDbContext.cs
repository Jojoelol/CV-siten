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

            // Konfigurera den nya kopplingstabellen
            modelBuilder.Entity<PersonProjekt>()
                .HasKey(pp => new { pp.PersonId, pp.ProjektId }); // Sammansatt nyckel

            modelBuilder.Entity<PersonProjekt>()
                .HasOne(pp => pp.Person)
                .WithMany(p => p.PersonProjekt)
                .HasForeignKey(pp => pp.PersonId);

            modelBuilder.Entity<PersonProjekt>()
                .HasOne(pp => pp.Projekt)
                .WithMany(p => p.PersonProjekt)
                .HasForeignKey(pp => pp.ProjektId);

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

            modelBuilder.Entity<Projekt>().HasData(new Projekt
            {
                Id = 1,
                Projektnamn = "Globalt CV-System",
                Beskrivning = "Ett system byggt i .NET 8 med SQL Server.",
                Startdatum = DateTimeOffset.Now,
                Slutdatum = DateTimeOffset.Now.AddMonths(1),
                Typ = "Webbutveckling",
                Status = "Pågående",
                //Fil = "exempel.pdf" // Det nya fältet
            });
        }
    }
}