using Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.IsPublished).HasDefaultValue(false);

                entity.HasIndex(e => e.Title).IsUnique();
                
                entity.HasIndex(e => e.Tags);
                
                entity.HasIndex(e => e.IsPublished);
            });
        }
    }
} 