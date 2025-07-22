using Microsoft.AspNetCore.Mvc;
using Core.Services;
using Shared.DTOs;

namespace Articles.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(JwtService jwtService, ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public ActionResult<TokenDto> Login(LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                if (_jwtService.ValidateCredentials(loginDto.Username, loginDto.Password))
                {
                    var token = _jwtService.GenerateToken(loginDto.Username);
                    _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
                    return Ok(token);
                }

                _logger.LogWarning("Failed login attempt for user {Username}", loginDto.Username);
                return Unauthorized(new { error = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("validate")]
        public ActionResult ValidateToken([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "Invalid token format" });
            }

            var token = authorization.Substring("Bearer ".Length);
            
            try
            {
                // Token validation logic would go here
                // For now, we'll just return OK if the token is present
                return Ok(new { message = "Token is valid" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Unauthorized(new { error = "Invalid token" });
            }
        }
    }
} 