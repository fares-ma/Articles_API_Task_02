using Articles.Api.Controllers;
using AutoMapper;
using Core.Services.Abstraction;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs;
using Shared.Models;

namespace Articles.Tests.Controllers;

public class ArticlesControllerTests
{
    private readonly Mock<IArticleService> _articleService = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ILogger<ArticlesController>> _logger = new();

    private ArticlesController CreateController() =>
        new(_articleService.Object, _mapper.Object, _logger.Object);

    [Fact]
    public async Task GetArticleByTitle_WithEmptyTitle_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.GetArticleByTitle(" ");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetArticlesByTag_WithEmptyTag_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.GetArticlesByTag("", 1, 10);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAllArticles_ShouldClampPaginationValues()
    {
        var controller = CreateController();
        PaginationParameters? captured = null;

        _articleService
            .Setup(s => s.GetAllArticlesAsync(It.IsAny<PaginationParameters>()))
            .Callback<PaginationParameters>(p => captured = p)
            .ReturnsAsync(new PaginationResult<Core.Domain.Models.Article>
            {
                Items = new List<Core.Domain.Models.Article>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 50,
                TotalPages = 0
            });

        var result = await controller.GetAllArticles(-10, 200);

        result.Result.Should().BeOfType<OkObjectResult>();
        captured.Should().NotBeNull();
        captured!.PageNumber.Should().Be(1);
        captured.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetArticleByTitle_WhenServiceUnavailable_DoesNotLeakInternalDetails()
    {
        var controller = CreateController();

        _articleService
            .Setup(s => s.GetArticleByTitleAsync("service-down"))
            .ThrowsAsync(new InvalidOperationException("internal detail should not leak"));

        var result = await controller.GetArticleByTitle("service-down");

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);

        var serialized = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
        serialized.Should().Contain("Service is currently unavailable");
        serialized.Should().NotContain("internal detail should not leak");
        serialized.Should().NotContain("details");
    }

    [Fact]
    public void ProtectedEndpoints_ShouldHaveAuthorizeAttribute()
    {
        var protectedMethodNames = new[]
        {
            nameof(ArticlesController.GetArticleById),
            nameof(ArticlesController.GetArticleByTitle),
            nameof(ArticlesController.GetArticlesByTag),
            nameof(ArticlesController.GetPublishedArticles),
            nameof(ArticlesController.CreateArticle),
            nameof(ArticlesController.UpdateArticle),
            nameof(ArticlesController.DeleteArticle),
            nameof(ArticlesController.GetArticlesByNewspaper)
        };

        foreach (var methodName in protectedMethodNames)
        {
            var method = typeof(ArticlesController).GetMethod(methodName);
            var hasAuthorize = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            hasAuthorize.Should().BeTrue($"{methodName} should be protected with [Authorize]");
        }
    }
}
