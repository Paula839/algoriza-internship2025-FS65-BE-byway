using Byway.Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace Byway.Infrastructure.Data
{
    public class BywayDbContext : DbContext
    {
        public BywayDbContext(DbContextOptions<BywayDbContext> options) : base(options)
        {

        }

        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BywayDbContext).Assembly);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            });

        }

    }
}
