using Articles.Api.Controllers;
using Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs;

namespace Articles.Tests.Controllers;

public class AuthControllerTests
{
    private static JwtService CreateJwtService()
    {
        var settings = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "THIS_IS_A_DEMO_SECRET_KEY_WITH_32_CHARS_MIN_12345",
            ["JwtSettings:Issuer"] = "ArticlesApiTests",
            ["JwtSettings:Audience"] = "ArticlesApiTestsAudience",
            ["JwtSettings:ExpirationInMinutes"] = "60",
            ["Auth:DemoUsers:0:Username"] = "admin",
            ["Auth:DemoUsers:0:Password"] = "password123"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new JwtService(configuration);
    }

    private static AuthController CreateController()
    {
        var logger = new Mock<ILogger<AuthController>>();
        return new AuthController(CreateJwtService(), logger.Object);
    }

    [Fact]
    public void Login_WithNullBody_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = controller.Login(null!);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Login_WithModelStateError_ReturnsValidationProblem()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Username", "Username is required");

        var result = controller.Login(new LoginDto { Username = "", Password = "" });

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.Value.Should().BeOfType<ValidationProblemDetails>();
        var details = objectResult.Value as ValidationProblemDetails;
        details!.Errors.Should().ContainKey("Username");
    }

    [Fact]
    public void Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var controller = CreateController();

        var result = controller.Login(new LoginDto
        {
            Username = "admin",
            Password = "wrong-password"
        });

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsToken()
    {
        var controller = CreateController();

        var result = controller.Login(new LoginDto
        {
            Username = "admin",
            Password = "password123"
        });

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeOfType<TokenDto>();
        var token = ok.Value as TokenDto;
        token!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ValidateToken_Endpoint_ShouldRequireAuthorize()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ValidateToken));

        var hasAuthorize = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Any();

        hasAuthorize.Should().BeTrue();
    }

    [Fact]
    public void ValidateTokenGet_ShouldBeRemoved_ForCleanApiSurface()
    {
        var method = typeof(AuthController).GetMethod("ValidateTokenGet");
        method.Should().BeNull();
    }
}
