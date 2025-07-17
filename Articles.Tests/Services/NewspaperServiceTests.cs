using Core.Domain.Models;
using Core.Services;
using Core.Services.Abstraction;
using FluentAssertions;
using Moq;
using Shared.Models;

namespace Articles.Tests.Services
{
   
    // Uses Moq to mock the repository and FluentAssertions for result verification
    public class NewspaperServiceTests
    {
        // Mock for the newspaper repository (no actual database connection)
        private readonly Mock<INewspaperRepository> _mockRepository;
        // The service instance under test
        private readonly NewspaperService _service;

        public NewspaperServiceTests()
        {
            // Initialize the mock and service before each test
            _mockRepository = new Mock<INewspaperRepository>();
            _service = new NewspaperService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllNewspapersAsync_ReturnsAllNewspapers()
        {
            // Arrange: Prepare test data and mock the repository
            var newspapers = new List<Newspaper>
            {
                new Newspaper { Id = 1, Name = "A" },
                new Newspaper { Id = 2, Name = "B" }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(newspapers);

            // Act: Call the method under test
            var result = await _service.GetAllNewspapersAsync();

            // Assert: Verify the result matches the test data
            result.Should().BeEquivalentTo(newspapers);
        }

        [Fact]
        public async Task GetAllNewspapersAsync_WithPagination_ReturnsPaginatedResult()
        {
            // Arrange: Prepare pagination data and mock the repository
            var parameters = new PaginationParameters { PageNumber = 1, PageSize = 2 };
            var pagedResult = new PaginationResult<Newspaper>
            {
                Items = new List<Newspaper> { new Newspaper { Id = 1, Name = "A" } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 1
            };
            _mockRepository.Setup(r => r.GetAllAsync(parameters)).ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetAllNewspapersAsync(parameters);

            // Assert
            result.Should().BeEquivalentTo(pagedResult);
        }

        [Fact]
        public async Task GetNewspaperByIdAsync_WithValidId_ReturnsNewspaper()
        {
            // Arrange: Prepare a test newspaper and mock the repository
            var newspaper = new Newspaper { Id = 1, Name = "A" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(newspaper);

            // Act
            var result = await _service.GetNewspaperByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(newspaper);
        }

        [Fact]
        public async Task GetNewspaperByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange: Mock the repository to return null for a non-existent newspaper
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Newspaper?)null);

            // Act
            var result = await _service.GetNewspaperByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetNewspaperByNameAsync_WithValidName_ReturnsNewspaper()
        {
            // Arrange: Prepare a test newspaper with a specific name
            var newspaper = new Newspaper { Id = 1, Name = "A" };
            _mockRepository.Setup(r => r.GetByNameAsync("A")).ReturnsAsync(newspaper);

            // Act
            var result = await _service.GetNewspaperByNameAsync("A");

            // Assert
            result.Should().BeEquivalentTo(newspaper);
        }

        [Fact]
        public async Task GetNewspaperByNameAsync_WithInvalidName_ReturnsNull()
        {
            // Arrange: Mock the repository to return null for a non-existent name
            _mockRepository.Setup(r => r.GetByNameAsync("X")).ReturnsAsync((Newspaper?)null);

            // Act
            var result = await _service.GetNewspaperByNameAsync("X");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveNewspapersAsync_ReturnsActiveNewspapers()
        {
            // Arrange: Prepare a list of active newspapers
            var active = new List<Newspaper> { new Newspaper { Id = 1, IsActive = true } };
            _mockRepository.Setup(r => r.GetActiveAsync()).ReturnsAsync(active);

            // Act
            var result = await _service.GetActiveNewspapersAsync();

            // Assert
            result.Should().BeEquivalentTo(active);
        }

        [Fact]
        public async Task CreateNewspaperAsync_SetsCreatedAtAndIsActive_AndCallsRepository()
        {
            // Arrange: Prepare a new newspaper and mock the add operation
            var input = new Newspaper { Name = "A" };
            var output = new Newspaper { Id = 1, Name = "A", CreatedAt = DateTime.UtcNow, IsActive = true };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Newspaper>())).ReturnsAsync(output);

            // Act
            var result = await _service.CreateNewspaperAsync(input);

            // Assert: Check IsActive and CreatedAt are set, and repository is called
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockRepository.Verify(r => r.AddAsync(It.Is<Newspaper>(n => n.IsActive && n.CreatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task UpdateNewspaperAsync_SetsUpdatedAt_AndCallsRepository()
        {
            // Arrange: Prepare a newspaper for update and mock the update operation
            var input = new Newspaper { Id = 1, Name = "A" };
            var updated = new Newspaper { Id = 1, Name = "A", UpdatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Newspaper>())).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateNewspaperAsync(input);

            // Assert: Check UpdatedAt is set and repository is called
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Newspaper>(n => n.UpdatedAt != null)), Times.Once);
        }

        [Fact]
        public async Task DeleteNewspaperAsync_CallsRepositoryAndReturnsResult()
        {
            // Arrange: Mock the delete operation
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteNewspaperAsync(1);

            // Assert: Check delete is called and result is correct
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsRepositoryResult()
        {
            // Arrange: Mock the existence check
            _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_ReturnsRepositoryResult()
        {
            // Arrange: Mock the existence check by name
            _mockRepository.Setup(r => r.ExistsByNameAsync("A")).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsByNameAsync("A");

            // Assert
            result.Should().BeTrue();
        }
    }
}
