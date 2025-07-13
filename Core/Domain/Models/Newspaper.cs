namespace Core.Domain.Models
{

    #region summary
    /// Newspaper entity represents a news publication or media outlet in the system.
    /// 
    /// Purpose:
    /// - Core domain model for newspaper/publisher management
    /// - Represents media organizations that publish articles
    /// - Supports one-to-many relationship with articles
    /// - Tracks publication metadata (founding date, status, contact info)
    /// 
    /// Dependencies:
    /// - Entity Framework Core for persistence
    /// - Article entity for one-to-many relationship
    /// - AutoMapper for DTO transformations
    /// 
    /// Alternatives:
    /// - Could implement magazine/journal types
    /// - Could add support for multiple locations
    /// - Could implement subscription models
    /// - Could add support for editorial teams

    #endregion
    public class Newspaper
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime FoundedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        #region Navigation Property - Relationship with Articles
        // Navigation property that represents the one-to-many relationship:
        // One newspaper can have many articles.
        // Entity Framework uses this to load all related articles for a newspaper.
        // 'virtual' allows for lazy loading if enabled.
        // Initialized as an empty list to avoid null reference issues.
        #endregion
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
} 