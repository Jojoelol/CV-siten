using CV_siten.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CV_siten.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PersonProject> PersonProjects { get; set; }
        public DbSet<CV> CVs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Tvinga tabellnamn (för att undvika "Invalid object name"-fel) ---
            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<PersonProject>().ToTable("PersonProjects");
            modelBuilder.Entity<Message>().ToTable("Messages");

            // --- Sammansatt nyckel för kopplingstabellen ---
            modelBuilder.Entity<PersonProject>()
                .HasKey(pp => new { pp.PersonId, pp.ProjectId });

            // --- Relationer för PersonProject ---
            modelBuilder.Entity<PersonProject>()
                .HasOne(pp => pp.Person)
                .WithMany(p => p.PersonProjects)
                .HasForeignKey(pp => pp.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonProject>()
                .HasOne(pp => pp.Project)
                .WithMany(p => p.PersonProjects)
                .HasForeignKey(pp => pp.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // FIX: Förhindrar cykliska raderingar

            // --- Relation för Projektägare ---
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.NoAction); // FIX: Förhindrar cykliska raderingar

            // --- Relationer för Meddelanden (Viktigt för att undvika felet du fick senast) ---
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // FIX: Förhindrar cykliska raderingar

            // --- Seed Data: Användare ---
            var hasher = new PasswordHasher<ApplicationUser>();

            // --- USER 1 ---
            var testUser = new ApplicationUser
            {
                Id = "test-user-1",
                UserName = "test@test.se",
                NormalizedUserName = "TEST@TEST.SE",
                Email = "test@test.se",
                NormalizedEmail = "TEST@TEST.SE",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
            testUser.PasswordHash = hasher.HashPassword(testUser, "Test123!");

            modelBuilder.Entity<ApplicationUser>().HasData(testUser);

            // --- Seed Data: Person ---
            modelBuilder.Entity<Person>().HasData(new Person
            {
                Id = 1,
                FirstName = "Joel",
                LastName = "Test",
                PhoneNumber = "0701234567",
                JobTitle = "Systemutvecklare",
                Description = "Testprofil.",
                Address = "Testvägen 1",
                PostalCode = "12345",
                City = "Teststad",
                Skills = "C#, ASP.NET Core, SQL",
                Education = "Örebro Universitet",
                Experience = "Junior utvecklare på Test AB",
                IsActive = true,
                IdentityUserId = "test-user-1",
                IsPrivate = false,
                ImageUrl = "Bild1.png"
            });

            // --- USER 2 ---
            var testUser2 = new ApplicationUser
            {
                Id = "test-user-2",
                UserName = "testsson@test.se",
                NormalizedUserName = "TESTSSON@TEST.SE",
                Email = "testsson@test.se",
                NormalizedEmail = "TESTSSON@TEST.SE",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
            testUser2.PasswordHash = hasher.HashPassword(testUser2, "Test123!");

            modelBuilder.Entity<ApplicationUser>().HasData(testUser2);

            modelBuilder.Entity<Person>().HasData(new Person
            {
                Id = 2,
                FirstName = "Oscar",
                LastName = "Test",
                PhoneNumber = "0709876543",
                JobTitle = "Systemutvecklare",
                Description = "Testprofil.",
                Address = "Testvägen 2",
                PostalCode = "12345",
                City = "Teststad",
                Skills = "C#, ASP.NET Core, SQL",
                Education = "Örebro Universitet",
                Experience = "Junior utvecklare på Test AB",
                IsActive = true,
                IdentityUserId = "test-user-2",
                IsPrivate = false,
                ImageUrl = "Bild1.png"
            });
        }
    }
}
