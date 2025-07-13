using Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data
{

    #region summary
    /// ApplicationDbContext represents the main database context for the Articles API.
    /// 
    /// Purpose:
    /// - Manages database connections and Entity Framework operations
    /// - Defines database schema and entity configurations
    /// - Handles relationships between entities (Articles and Newspapers)
    /// - Provides DbSet properties for data access
    /// 
    /// Dependencies:
    /// - Entity Framework Core for ORM functionality
    /// - Core.Domain.Models for entity definitions
    /// - SQL Server as the database provider
    /// 
    /// Alternatives:
    /// - Could use different database providers (PostgreSQL, MySQL, SQLite)
    /// - Could implement separate contexts for different bounded contexts
    /// - Could use different ORM frameworks (Dapper, NHibernate) 
    #endregion

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Newspaper> Newspapers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Newspaper table
            modelBuilder.Entity<Newspaper>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Publisher).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Website).HasMaxLength(500);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
                entity.Property(e => e.FoundedDate).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Article table with Newspaper relationship
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
                entity.HasIndex(e => e.NewspaperId);

                // Relationship with Newspaper
                entity.HasOne(e => e.Newspaper)
                      .WithMany(n => n.Articles)
                      .HasForeignKey(e => e.NewspaperId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
} 