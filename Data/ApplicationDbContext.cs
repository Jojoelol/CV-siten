using CV_siten.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Projekt> Projekt { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<CV> CVer { get; set; }

        // Lägg till denna metod här under:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Avsandare)
                .WithMany()
                .HasForeignKey(m => m.AvsandareId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Mottagare)
                .WithMany()
                .HasForeignKey(m => m.MottagareId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // --- Skapa testanvändare (IdentityUser) ---
            var hasher = new PasswordHasher<ApplicationUser>();

            var testUser = new ApplicationUser
            {
                Id = "test-user-1", // Måste vara en sträng
                UserName = "test@test.se",
                NormalizedUserName = "TEST@TEST.SE",
                Email = "test@test.se",
                NormalizedEmail = "TEST@TEST.SE",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            // Hasha lösenordet
            testUser.PasswordHash = hasher.HashPassword(testUser, "Test123!");

            // Lägg in användaren i databasen
            modelBuilder.Entity<ApplicationUser>().HasData(testUser);

            // --- Skapa Person kopplad till IdentityUser ---
            modelBuilder.Entity<Person>().HasData(new Person
            {
                Id = 1,
                Fornamn = "Joel",
                Efternamn = "Test",
                Yrkestitel = "Systemutvecklare",
                Beskrivning = "Detta är en testprofil skapad via kod.",
                AktivtKonto = true,
                Telefonnummer = "0701234567",
                BildUrl = "",
                IdentityUserId = "test-user-1" // Koppling till ApplicationUser
            });

        }
    }
}