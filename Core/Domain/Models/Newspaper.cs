namespace Core.Domain.Models
{
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