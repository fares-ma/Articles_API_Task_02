using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Services;

namespace Articles.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<TestController> _logger;

        public TestController(JwtService jwtService, ILogger<TestController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        [HttpGet("jwt-test")]
        public IActionResult JwtTest()
        {
            var token = _jwtService.GenerateToken("admin");
            return Ok(new { 
                message = "JWT Token generated for testing",
                token = token,
                instructions = "Use this token in Authorization header: Bearer {token}"
            });
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var username = User.Identity?.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new { 
                message = "This is a protected endpoint",
                username = username,
                claims = claims,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var username = User.Identity?.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new { 
                message = "Token is valid",
                username = username,
                claims = claims,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("s3-test")]
        public IActionResult S3Test()
        {
            return Ok(new { 
                message = "S3 integration test endpoint",
                useS3 = false, // This will be false unless configured
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("auth-info")]
        public IActionResult AuthInfo()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var username = User.Identity?.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new { 
                isAuthenticated = isAuthenticated,
                username = username,
                claims = claims,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("login-test")]
        public IActionResult LoginTest()
        {
            return Ok(new { 
                message = "Test login credentials",
                credentials = new { 
                    username = "admin", 
                    password = "password123" 
                },
                instructions = "Use these credentials with POST /api/Auth/login"
            });
        }
    }
}