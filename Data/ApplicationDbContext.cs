using CV_siten.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CV_siten.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Meddelande> Messages { get; set; }
        public DbSet<Projekt> Projects { get; set; }
    }
}
