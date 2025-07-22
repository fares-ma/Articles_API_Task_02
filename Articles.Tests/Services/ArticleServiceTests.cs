using Core.Domain.Models;
using Core.Services;
using Core.Services.Abstraction;
using FluentAssertions;
using Moq;
using Shared.Models;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Amazon.S3;

namespace Articles.Tests.Services
{

    #region summary
    /// ArticleServiceTests provides unit tests for ArticleService business logic.
    /// 
    /// Purpose:
    /// - Tests article business logic and validation rules
    /// - Verifies CRUD operations with proper error handling
    /// - Tests pagination and filtering functionality
    /// - Ensures data integrity and business rule enforcement
    /// - Validates service layer behavior in isolation
    /// 
    /// Dependencies:
    /// - xUnit for test framework
    /// - Moq for mocking dependencies
    /// - FluentAssertions for readable assertions
    /// - Core.Services for service under test
    /// 
    /// Alternatives:
    /// - Could implement integration tests with real database
    /// - Could use different mocking frameworks (NSubstitute, FakeItEasy)
    /// - Could implement property-based testing
    /// - Could add performance testing scenarios

    #endregion
    public class ArticleServiceTests
    {
        private readonly Mock<IArticleRepository> _mockRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly ArticleService _service;

        public ArticleServiceTests()
        {
            _mockRepository = new Mock<IArticleRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _mockCache = new Mock<IMemoryCache>();
            
            // Setup configuration section for UseS3
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("false");
            _mockConfiguration.Setup(c => c["UseS3"]).Returns("false");
            
            // Setup other required configuration values
            _mockConfiguration.Setup(c => c["AWS:BucketName"]).Returns("test-bucket");
            _mockConfiguration.Setup(c => c["AWS:ArticlesKey"]).Returns("articles/test.json");
            
            // Setup memory cache
            var cacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);
            
            _service = new ArticleService(
                _mockRepository.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockCache.Object);
        }

        [Fact]
        public async Task GetArticleByIdAsync_WithValidId_ReturnsArticle()
        {
            // Arrange
            var expectedArticle = new Article { Id = 1, Title = "Test Article" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedArticle);

            // Act
            var result = await _service.GetArticleByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedArticle);
        }

        [Fact]
        public async Task GetArticleByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Article?)null);

            // Act
            var result = await _service.GetArticleByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllArticlesAsync_WithPagination_ReturnsCorrectResult()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article { Id = 1, Title = "Article 1" },
                new Article { Id = 2, Title = "Article 2" },
                new Article { Id = 3, Title = "Article 3" }
            };

            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 2 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(articles);
            _mockRepository.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(3);

            // Act
            var result = await _service.GetAllArticlesAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetPublishedArticlesAsync_ReturnsOnlyPublishedArticles()
        {
            // Arrange
            var publishedArticles = new List<Article>
            {
                new Article { Id = 1, Title = "Published 1", IsPublished = true },
                new Article { Id = 2, Title = "Published 2", IsPublished = true }
            };

            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10 };
            _mockRepository.Setup(r => r.GetPublishedAsync()).ReturnsAsync(publishedArticles);
            _mockRepository.Setup(r => r.GetPublishedCountAsync()).ReturnsAsync(2);

            // Act
            var result = await _service.GetPublishedArticlesAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(a => a.IsPublished);
        }
    }
} 