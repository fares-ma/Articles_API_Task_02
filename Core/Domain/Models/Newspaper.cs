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

        // Navigation Property - Relationship with Articles
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
} 