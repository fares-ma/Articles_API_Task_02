using Core.Domain.Models;
using Core.Services;
using Core.Services.Abstraction;
using FluentAssertions;
using Moq;
using Shared.Models;

namespace Articles.Tests.Services
{
    public class NewspaperServiceTests
    {
        private readonly Mock<INewspaperRepository> _mockRepository;
        private readonly NewspaperService _service;

        public NewspaperServiceTests()
        {
            _mockRepository = new Mock<INewspaperRepository>();
            _service = new NewspaperService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllNewspapersAsync_ReturnsAllNewspapers()
        {
            var newspapers = new List<Newspaper>
            {
                new Newspaper { Id = 1, Name = "A" },
                new Newspaper { Id = 2, Name = "B" }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(newspapers);

            var result = await _service.GetAllNewspapersAsync();

            result.Should().BeEquivalentTo(newspapers);
        }

        [Fact]
        public async Task GetAllNewspapersAsync_WithPagination_ReturnsPaginatedResult()
        {
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

            var result = await _service.GetAllNewspapersAsync(parameters);

            result.Should().BeEquivalentTo(pagedResult);
        }

        [Fact]
        public async Task GetNewspaperByIdAsync_WithValidId_ReturnsNewspaper()
        {
            var newspaper = new Newspaper { Id = 1, Name = "A" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(newspaper);

            var result = await _service.GetNewspaperByIdAsync(1);

            result.Should().BeEquivalentTo(newspaper);
        }

        [Fact]
        public async Task GetNewspaperByIdAsync_WithInvalidId_ReturnsNull()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Newspaper?)null);

            var result = await _service.GetNewspaperByIdAsync(99);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetNewspaperByNameAsync_WithValidName_ReturnsNewspaper()
        {
            var newspaper = new Newspaper { Id = 1, Name = "A" };
            _mockRepository.Setup(r => r.GetByNameAsync("A")).ReturnsAsync(newspaper);

            var result = await _service.GetNewspaperByNameAsync("A");

            result.Should().BeEquivalentTo(newspaper);
        }

        [Fact]
        public async Task GetNewspaperByNameAsync_WithInvalidName_ReturnsNull()
        {
            _mockRepository.Setup(r => r.GetByNameAsync("X")).ReturnsAsync((Newspaper?)null);

            var result = await _service.GetNewspaperByNameAsync("X");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveNewspapersAsync_ReturnsActiveNewspapers()
        {
            var active = new List<Newspaper> { new Newspaper { Id = 1, IsActive = true } };
            _mockRepository.Setup(r => r.GetActiveAsync()).ReturnsAsync(active);

            var result = await _service.GetActiveNewspapersAsync();

            result.Should().BeEquivalentTo(active);
        }

        [Fact]
        public async Task CreateNewspaperAsync_SetsCreatedAtAndIsActive_AndCallsRepository()
        {
            var input = new Newspaper { Name = "A" };
            var output = new Newspaper { Id = 1, Name = "A", CreatedAt = DateTime.UtcNow, IsActive = true };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Newspaper>())).ReturnsAsync(output);

            var result = await _service.CreateNewspaperAsync(input);

            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockRepository.Verify(r => r.AddAsync(It.Is<Newspaper>(n => n.IsActive && n.CreatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task UpdateNewspaperAsync_SetsUpdatedAt_AndCallsRepository()
        {
            var input = new Newspaper { Id = 1, Name = "A" };
            var updated = new Newspaper { Id = 1, Name = "A", UpdatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Newspaper>())).ReturnsAsync(updated);

            var result = await _service.UpdateNewspaperAsync(input);

            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Newspaper>(n => n.UpdatedAt != null)), Times.Once);
        }

        [Fact]
        public async Task DeleteNewspaperAsync_CallsRepositoryAndReturnsResult()
        {
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteNewspaperAsync(1);

            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsRepositoryResult()
        {
            _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var result = await _service.ExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_ReturnsRepositoryResult()
        {
            _mockRepository.Setup(r => r.ExistsByNameAsync("A")).ReturnsAsync(true);

            var result = await _service.ExistsByNameAsync("A");

            result.Should().BeTrue();
        }
    }
}
