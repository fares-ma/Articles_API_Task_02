using Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Check if data already exists
            if (await context.Articles.AnyAsync())
            {
                return; // Data already seeded
            }

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Getting Started with ASP.NET Core",
                    Description = "A comprehensive guide to building web applications with ASP.NET Core",
                    Content = "ASP.NET Core is a cross-platform, high-performance, open-source framework for building modern, cloud-enabled, Internet-connected applications. This article covers the basics of getting started with ASP.NET Core, including project setup, routing, and dependency injection.",
                    Tags = "ASP.NET Core,Web Development,C#",
                    Author = "John Smith",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsPublished = true,
                    ViewCount = 1250
                },
                new Article
                {
                    Title = "Entity Framework Core Best Practices",
                    Description = "Learn the best practices for using Entity Framework Core in your applications",
                    Content = "Entity Framework Core is Microsoft's modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations. This article discusses performance optimization, query efficiency, and common pitfalls to avoid.",
                    Tags = "Entity Framework,Database,ORM",
                    Author = "Sarah Johnson",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    IsPublished = true,
                    ViewCount = 890
                },
                new Article
                {
                    Title = "Building RESTful APIs with ASP.NET Core",
                    Description = "Complete guide to creating RESTful APIs using ASP.NET Core Web API",
                    Content = "RESTful APIs are the backbone of modern web applications. This article demonstrates how to build robust, scalable APIs using ASP.NET Core Web API. Topics include controller design, HTTP methods, status codes, and API documentation with Swagger.",
                    Tags = "REST API,Web API,ASP.NET Core",
                    Author = "Michael Brown",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    IsPublished = true,
                    ViewCount = 2100
                },
                new Article
                {
                    Title = "Dependency Injection in .NET",
                    Description = "Understanding dependency injection and its implementation in .NET applications",
                    Content = "Dependency Injection (DI) is a design pattern that implements Inversion of Control (IoC) for managing dependencies. This article explains the concept of DI, its benefits, and how to implement it effectively in .NET applications using the built-in DI container.",
                    Tags = "Dependency Injection,IoC,.NET",
                    Author = "Emily Davis",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    IsPublished = true,
                    ViewCount = 750
                },
                new Article
                {
                    Title = "Microservices Architecture with .NET",
                    Description = "Exploring microservices architecture and implementation strategies",
                    Content = "Microservices architecture is a software development approach where applications are built as a collection of loosely coupled, independently deployable services. This article covers the fundamentals of microservices, communication patterns, and deployment strategies using .NET technologies.",
                    Tags = "Microservices,Architecture,.NET",
                    Author = "David Wilson",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    IsPublished = false,
                    ViewCount = 0
                },
                new Article
                {
                    Title = "C# Advanced Features and Techniques",
                    Description = "Deep dive into advanced C# features and programming techniques",
                    Content = "C# is a powerful, modern programming language with many advanced features. This article explores async/await patterns, LINQ optimization, reflection, and other advanced techniques that can help you write more efficient and maintainable code.",
                    Tags = "C#,Programming,Advanced Features",
                    Author = "Lisa Anderson",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    IsPublished = true,
                    ViewCount = 320
                },
                new Article
                {
                    Title = "Database Design Principles",
                    Description = "Essential principles for designing efficient and scalable databases",
                    Content = "Good database design is crucial for application performance and maintainability. This article covers normalization, indexing strategies, relationship design, and best practices for creating robust database schemas that can handle growth and change.",
                    Tags = "Database Design,SQL,Performance",
                    Author = "Robert Taylor",
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    IsPublished = true,
                    ViewCount = 180
                },
                new Article
                {
                    Title = "Testing Strategies for .NET Applications",
                    Description = "Comprehensive guide to testing .NET applications using various frameworks",
                    Content = "Testing is an essential part of software development. This article covers unit testing with MSTest and xUnit, integration testing, and automated testing strategies. Learn how to write effective tests that improve code quality and reduce bugs.",
                    Tags = "Testing,Unit Tests,Integration Tests",
                    Author = "Jennifer Martinez",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    IsPublished = true,
                    ViewCount = 95
                }
            };

            await context.Articles.AddRangeAsync(articles);
            await context.SaveChangesAsync();
        }
    }
} 