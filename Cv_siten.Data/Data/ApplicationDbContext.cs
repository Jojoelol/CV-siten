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

            modelBuilder.Entity<PersonProject>()
                .HasKey(pp => new { pp.PersonId, pp.ProjectId });

            modelBuilder.Entity<PersonProject>()
                .HasOne(pp => pp.Person)
                .WithMany(p => p.PersonProjects)
                .HasForeignKey(pp => pp.PersonId);

            modelBuilder.Entity<PersonProject>()
                .HasOne(pp => pp.Project)
                .WithMany(p => p.PersonProjects)
                .HasForeignKey(pp => pp.ProjectId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            var hasher = new PasswordHasher<ApplicationUser>();

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
                IsPrivate = false
            });
        }
    }
}