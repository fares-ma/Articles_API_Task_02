using Core.Services;
using Core.Services.Abstraction;
using Core.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using FluentAssertions;
using Shared.Models;
using AutoMapper;
using Core.Domain.Exceptions;

namespace Articles.Tests.Services
{
    public class ArticleServiceTests
    {
        private readonly Mock<IArticleRepository> _mockRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IS3ArticleProvider> _mockS3ArticleProvider;
        private readonly Mock<IS3FileProvider> _mockS3FileProvider;
        private readonly ArticleService _service;

        public ArticleServiceTests()
        {
            _mockRepository = new Mock<IArticleRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _mockCache = new Mock<IMemoryCache>();
            _mockS3ArticleProvider = new Mock<IS3ArticleProvider>();
            _mockS3FileProvider = new Mock<IS3FileProvider>();

            
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
                _mockCache.Object,
                _mockS3ArticleProvider.Object,
                _mockS3FileProvider.Object);
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
            var paginationResult = new PaginationResult<Article>
            {
                Items = articles.Take(2),
                TotalCount = 3,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 2
            };
            
            _mockRepository.Setup(r => r.GetAllAsync(parameters)).ReturnsAsync(paginationResult);

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
            var paginationResult = new PaginationResult<Article>
            {
                Items = publishedArticles,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1
            };
            
            _mockRepository.Setup(r => r.GetPublishedAsync(parameters)).ReturnsAsync(paginationResult);

            // Act
            var result = await _service.GetPublishedArticlesAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(a => a.IsPublished);
        }

        [Fact]
        public async Task CreateArticleAsync_WithDuplicateTitle_ThrowsDuplicateEntityException()
        {
            // Arrange
            var article = new Article { Title = "Duplicate Title" };
            _mockRepository.Setup(r => r.ExistsByTitleAsync("Duplicate Title")).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DuplicateEntityException>(
                () => _service.CreateArticleAsync(article));

            exception.Message.Should().Contain("Article with title 'Duplicate Title' already exists");
            exception.EntityName.Should().Be("Article");
            exception.PropertyName.Should().Be("title");
            exception.PropertyValue.Should().Be("Duplicate Title");
        }

        [Fact]
        public async Task UpdateArticleAsync_WithNonExistentId_ThrowsArticleNotFoundException()
        {
            // Arrange
            var article = new Article { Id = 999, Title = "Updated Title" };
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Article?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArticleNotFoundException>(
                () => _service.UpdateArticleAsync(article));

            exception.Message.Should().Contain("Article with ID 999 not found");
        }

        [Fact]
        public async Task UpdateArticleAsync_WithDuplicateTitle_ThrowsDuplicateEntityException()
        {
            // Arrange
            var existingArticle = new Article { Id = 1, Title = "Original Title" };
            var updatedArticle = new Article { Id = 1, Title = "Duplicate Title" };
            
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingArticle);
            _mockRepository.Setup(r => r.ExistsByTitleAsync("Duplicate Title")).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DuplicateEntityException>(
                () => _service.UpdateArticleAsync(updatedArticle));

            exception.Message.Should().Contain("Article with title 'Duplicate Title' already exists");
        }

        [Fact]
        public async Task DeleteArticleAsync_WithNonExistentId_ThrowsArticleNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArticleNotFoundException>(
                () => _service.DeleteArticleAsync(999));

            exception.Message.Should().Contain("Article with ID 999 not found");
        }

        [Fact]
        public async Task CreateArticleAsync_WithValidData_SetsCreatedAtAndViewCount()
        {
            // Arrange
            var article = new Article { Title = "New Article" };
            var createdArticle = new Article { Id = 1, Title = "New Article", CreatedAt = DateTime.UtcNow, ViewCount = 0 };
            
            _mockRepository.Setup(r => r.ExistsByTitleAsync("New Article")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Article>())).ReturnsAsync(createdArticle);

            // Act
            var result = await _service.CreateArticleAsync(article);

            // Assert
            result.Should().NotBeNull();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.ViewCount.Should().Be(0);
            
            _mockRepository.Verify(r => r.AddAsync(It.Is<Article>(a => 
                a.CreatedAt != default && a.ViewCount == 0)), Times.Once);
        }

        [Fact]
        public async Task UpdateArticleAsync_WithValidData_SetsUpdatedAtAndPreservesOriginalData()
        {
            // Arrange
            var existingArticle = new Article { Id = 1, Title = "Original Title", CreatedAt = DateTime.UtcNow.AddDays(-1), ViewCount = 5 };
            var updatedArticle = new Article { Id = 1, Title = "Updated Title" };
            var resultArticle = new Article { Id = 1, Title = "Updated Title", UpdatedAt = DateTime.UtcNow };
            
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingArticle);
            _mockRepository.Setup(r => r.ExistsByTitleAsync("Updated Title")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Article>())).ReturnsAsync(resultArticle);

            // Act
            var result = await _service.UpdateArticleAsync(updatedArticle);

            // Assert
            result.Should().NotBeNull();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Article>(a => 
                a.UpdatedAt != default && 
                a.CreatedAt == existingArticle.CreatedAt && 
                a.ViewCount == existingArticle.ViewCount)), Times.Once);
        }
    }
} 