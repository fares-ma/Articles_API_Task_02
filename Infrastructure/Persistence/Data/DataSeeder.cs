using Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data
{

    #region summary
    /// DataSeeder class responsible for populating the database with initial test data.
    /// This ensures the application has sample data for testing and demonstration purposes.
    /// 
    /// Purpose:
    /// - Provides realistic test data for development and testing
    /// - Demonstrates the relationship between newspapers and articles
    /// - Ensures API endpoints have data to work with
    /// 
    /// Dependencies:
    /// - ApplicationDbContext for database operations
    /// - Core.Domain.Models for entity definitions
    /// 
    /// Alternatives:
    /// - Could use external data files (JSON/CSV)
    /// - Could implement data factories for more complex scenarios
    /// - Could use database seeding tools like Bogus for fake data generation 
    #endregion
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Check if data already exists to avoid duplicate seeding
            if (await context.Newspapers.AnyAsync())
            {
                Console.WriteLine("Newspapers already seeded, skipping...");
                return; // Newspapers already seeded
            }

            // Seed Newspapers first (parent entities)
            var newspapers = new List<Newspaper>
            {
                new Newspaper
                {
                    Name = "Tech Daily",
                    Description = "Leading technology news and insights for developers and IT professionals",
                    Publisher = "Tech Media Group",
                    Website = "https://techdaily.com",
                    LogoUrl = "https://techdaily.com/logo.png",
                    FoundedDate = new DateTime(2018, 3, 15),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsActive = true
                },
                new Newspaper
                {
                    Name = "Business Weekly",
                    Description = "Comprehensive business news, market analysis, and economic insights",
                    Publisher = "Business Publications Ltd",
                    Website = "https://businessweekly.com",
                    LogoUrl = "https://businessweekly.com/logo.png",
                    FoundedDate = new DateTime(2015, 7, 22),
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    IsActive = true
                },
                new Newspaper
                {
                    Name = "Science Today",
                    Description = "Latest scientific discoveries, research breakthroughs, and innovation news",
                    Publisher = "Science Media Network",
                    Website = "https://sciencetoday.com",
                    LogoUrl = "https://sciencetoday.com/logo.png",
                    FoundedDate = new DateTime(2020, 1, 10),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    IsActive = true
                },
                new Newspaper
                {
                    Name = "Sports Central",
                    Description = "Comprehensive sports coverage, analysis, and athlete interviews",
                    Publisher = "Sports Network International",
                    Website = "https://sportscentral.com",
                    LogoUrl = "https://sportscentral.com/logo.png",
                    FoundedDate = new DateTime(2016, 11, 8),
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    IsActive = true
                },
                new Newspaper
                {
                    Name = "Health & Wellness",
                    Description = "Medical research, health tips, and wellness lifestyle guidance",
                    Publisher = "Health Media Group",
                    Website = "https://healthwellness.com",
                    LogoUrl = "https://healthwellness.com/logo.png",
                    FoundedDate = new DateTime(2019, 5, 12),
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsActive = true
                }
            };

            await context.Newspapers.AddRangeAsync(newspapers);
            await context.SaveChangesAsync();

            Console.WriteLine($"Newspapers seeded successfully with {newspapers.Count} newspapers.");
            
            // Also seed articles if they don't exist
            await SeedArticlesIfNeededAsync(context);
        }

        private static async Task SeedArticlesIfNeededAsync(ApplicationDbContext context)
        {
            if (await context.Articles.AnyAsync())
            {
                Console.WriteLine("Articles already seeded, skipping...");
                return;
            }

            // Get existing newspapers for relationships
            var newspapers = await context.Newspapers.ToListAsync();
            if (!newspapers.Any())
            {
                Console.WriteLine("No newspapers found, cannot seed articles with relationships.");
                return;
            }

            var techDaily = newspapers.FirstOrDefault(n => n.Name == "Tech Daily");
            var businessWeekly = newspapers.FirstOrDefault(n => n.Name == "Business Weekly");
            var scienceToday = newspapers.FirstOrDefault(n => n.Name == "Science Today");
            var sportsCentral = newspapers.FirstOrDefault(n => n.Name == "Sports Central");
            var healthWellness = newspapers.FirstOrDefault(n => n.Name == "Health & Wellness");

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Getting Started with ASP.NET Core 8",
                    Description = "A comprehensive guide to building modern web applications with ASP.NET Core 8",
                    Content = "ASP.NET Core 8 introduces several new features including improved performance, enhanced security, and better developer experience. This article covers the basics of getting started with ASP.NET Core 8, including project setup, routing, dependency injection, and best practices for building scalable web applications.",
                    Tags = "ASP.NET Core,Web Development,C#,Microsoft",
                    Author = "John Smith",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsPublished = true,
                    ViewCount = 1250,
                    NewspaperId = techDaily?.Id
                },
                new Article
                {
                    Title = "Entity Framework Core Best Practices",
                    Description = "Learn the best practices for using Entity Framework Core in production applications",
                    Content = "Entity Framework Core is Microsoft's modern object-database mapper for .NET. This article discusses performance optimization techniques, query efficiency strategies, common pitfalls to avoid, and advanced features like lazy loading, change tracking, and migrations. Learn how to build robust data access layers that scale with your application.",
                    Tags = "Entity Framework,Database,ORM,Performance",
                    Author = "Sarah Johnson",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    IsPublished = true,
                    ViewCount = 890,
                    NewspaperId = techDaily?.Id
                },
                new Article
                {
                    Title = "Microservices Architecture with .NET",
                    Description = "Exploring microservices architecture and implementation strategies using .NET technologies",
                    Content = "Microservices architecture is a software development approach where applications are built as a collection of loosely coupled, independently deployable services. This article covers the fundamentals of microservices, communication patterns, deployment strategies, and how to implement them effectively using .NET technologies including Docker and Kubernetes.",
                    Tags = "Microservices,Architecture,.NET,Docker",
                    Author = "David Wilson",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    IsPublished = false,
                    ViewCount = 0,
                    NewspaperId = techDaily?.Id
                },
                new Article
                {
                    Title = "The Future of Digital Banking",
                    Description = "How technology is transforming the banking industry and customer experience",
                    Content = "Digital banking is revolutionizing how financial institutions operate and serve their customers. This article explores the latest trends in fintech, including mobile banking, blockchain technology, AI-powered financial services, and the impact of digital transformation on traditional banking models.",
                    Tags = "Banking,Fintech,Digital Transformation,Technology",
                    Author = "Emily Davis",
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    IsPublished = true,
                    ViewCount = 2100,
                    NewspaperId = businessWeekly?.Id
                },
                new Article
                {
                    Title = "Sustainable Business Practices",
                    Description = "Implementing eco-friendly strategies for long-term business success",
                    Content = "Sustainability is no longer optional for businesses. This article examines how companies can implement sustainable practices while maintaining profitability. Topics include green supply chains, renewable energy adoption, waste reduction strategies, and how sustainability initiatives can enhance brand reputation and customer loyalty.",
                    Tags = "Sustainability,Business Strategy,Environment,CSR",
                    Author = "Michael Brown",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    IsPublished = true,
                    ViewCount = 750,
                    NewspaperId = businessWeekly?.Id
                },
                new Article
                {
                    Title = "Breakthrough in Quantum Computing",
                    Description = "Recent advances in quantum computing technology and its potential applications",
                    Content = "Quantum computing represents the next frontier in computational technology. This article discusses recent breakthroughs in quantum computing, including improved qubit stability, error correction methods, and potential applications in cryptography, drug discovery, and complex optimization problems.",
                    Tags = "Quantum Computing,Technology,Research,Innovation",
                    Author = "Dr. Lisa Anderson",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    IsPublished = true,
                    ViewCount = 3200,
                    NewspaperId = scienceToday?.Id
                },
                new Article
                {
                    Title = "Climate Change Research Update",
                    Description = "Latest findings in climate science and environmental research",
                    Content = "Climate change research continues to reveal new insights about our planet's changing environment. This article presents the latest findings from climate scientists, including temperature trends, sea level rise data, and the effectiveness of various mitigation strategies.",
                    Tags = "Climate Change,Environment,Research,Science",
                    Author = "Dr. Robert Taylor",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    IsPublished = true,
                    ViewCount = 1800,
                    NewspaperId = scienceToday?.Id
                },
                new Article
                {
                    Title = "The Evolution of Sports Analytics",
                    Description = "How data science is transforming sports performance and strategy",
                    Content = "Sports analytics has revolutionized how teams analyze performance, develop strategies, and make decisions. This article explores the latest trends in sports analytics, including player tracking technology, performance metrics, and how data-driven insights are changing the game across all major sports.",
                    Tags = "Sports Analytics,Data Science,Performance,Technology",
                    Author = "Jennifer Martinez",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    IsPublished = true,
                    ViewCount = 950,
                    NewspaperId = sportsCentral?.Id
                },
                new Article
                {
                    Title = "Mental Health in Professional Sports",
                    Description = "Addressing the psychological challenges faced by professional athletes",
                    Content = "Mental health awareness in professional sports has gained significant attention in recent years. This article examines the psychological challenges faced by athletes, the importance of mental health support, and how sports organizations are implementing programs to support athlete well-being.",
                    Tags = "Mental Health,Sports Psychology,Athletes,Wellness",
                    Author = "Dr. Sarah Williams",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    IsPublished = true,
                    ViewCount = 680,
                    NewspaperId = sportsCentral?.Id
                },
                new Article
                {
                    Title = "Advances in Personalized Medicine",
                    Description = "How genetic testing and AI are revolutionizing healthcare",
                    Content = "Personalized medicine represents a paradigm shift in healthcare, where treatments are tailored to individual genetic profiles and health characteristics. This article explores recent advances in genetic testing, AI-powered diagnostics, and how personalized medicine is improving patient outcomes across various medical conditions.",
                    Tags = "Personalized Medicine,Healthcare,Genetics,AI",
                    Author = "Dr. James Chen",
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    IsPublished = true,
                    ViewCount = 1450,
                    NewspaperId = healthWellness?.Id
                },
                new Article
                {
                    Title = "The Science of Sleep Optimization",
                    Description = "Research-backed strategies for improving sleep quality and health",
                    Content = "Quality sleep is fundamental to overall health and well-being. This article examines the latest research on sleep science, including circadian rhythm optimization, sleep hygiene practices, and how technology can help improve sleep quality. Learn evidence-based strategies for better sleep.",
                    Tags = "Sleep,Health,Wellness,Research",
                    Author = "Dr. Amanda Rodriguez",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    IsPublished = true,
                    ViewCount = 1120,
                    NewspaperId = healthWellness?.Id
                },
                new Article
                {
                    Title = "Database Design Principles",
                    Description = "Essential principles for designing efficient and scalable databases",
                    Content = "Good database design is crucial for application performance and maintainability. This article covers normalization, indexing strategies, relationship design, and best practices for creating robust database schemas that can handle growth and change. Learn how to design databases that scale with your application needs.",
                    Tags = "Database Design,SQL,Performance,Architecture",
                    Author = "Robert Taylor",
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    IsPublished = true,
                    ViewCount = 180,
                    NewspaperId = null // Demonstrating optional relationship
                },
                new Article
                {
                    Title = "Testing Strategies for .NET Applications",
                    Description = "Comprehensive guide to testing .NET applications using various frameworks",
                    Content = "Testing is an essential part of software development that ensures code quality and reduces bugs. This article covers unit testing with MSTest and xUnit, integration testing strategies, and automated testing approaches. Learn how to write effective tests that improve code quality and maintainability.",
                    Tags = "Testing,Unit Tests,Integration Tests,.NET",
                    Author = "Jennifer Martinez",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    IsPublished = true,
                    ViewCount = 95,
                    NewspaperId = null // Demonstrating optional relationship
                }
            };

            await context.Articles.AddRangeAsync(articles);
            await context.SaveChangesAsync();
            Console.WriteLine($"Articles seeded successfully with {articles.Count} articles.");
        }
    }
} 