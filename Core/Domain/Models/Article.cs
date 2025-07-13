namespace Core.Domain.Models
{
    #region summary

    /// Article entity represents a news article or blog post in the system.
    /// 
    /// Purpose:
    /// - Core domain model for article management
    /// - Represents the main content entity in the application
    /// - Supports optional relationship with newspapers
    /// - Tracks article metadata (views, publication status, timestamps)
    /// 
    /// Dependencies:
    /// - Entity Framework Core for persistence
    /// - Newspaper entity for optional relationship
    /// - AutoMapper for DTO transformations
    /// 
    /// Alternatives:
    /// - Could implement content versioning
    /// - Could add support for rich media content
    /// - Could implement article categories or topics
    /// - Could add support for multiple authors 
    #endregion
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // string.Empty => Null Reference Exceptions = 0
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        
        // Foreign Key for Newspaper relationship
        public int? NewspaperId { get; set; }

        #region Navigation Property
        // Navigation property that links this article to its related newspaper.
        // This allows access to the full Newspaper object (e.g., article.Newspaper.Name)
        // Used by Entity Framework to manage relationships between tables.
        // 'virtual' enables lazy loading (optional), and the property is nullable,
        // meaning an article may or may not belong to a newspaper.
        #endregion

        public virtual Newspaper? Newspaper { get; set; }
    }
} 