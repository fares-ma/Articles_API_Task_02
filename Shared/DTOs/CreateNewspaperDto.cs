namespace Shared.DTOs
{
    public class CreateNewspaperDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime FoundedDate { get; set; }
    }
} 