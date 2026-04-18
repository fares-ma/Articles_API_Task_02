using Microsoft.AspNetCore.Mvc;
using Core.Services;
using Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

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
        public ActionResult<TokenDto> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var username = loginDto.Username.Trim();
            var password = loginDto.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { error = "Username and password are required" });
            }

            if (_jwtService.ValidateCredentials(username, password))
            {
                var token = _jwtService.GenerateToken(username);
                _logger.LogInformation("User {Username} logged in successfully", username);
                return Ok(token);
            }

            _logger.LogWarning("Failed login attempt for user {Username}", username);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        [HttpPost("validate")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            var username = User.Identity?.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            _logger.LogInformation("Token validated successfully for user {Username}", username);
            
            return Ok(new 
            { 
                message = "Token is valid",
                username = username,
                claims = claims,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("validate-header")]
        public ActionResult ValidateTokenFromHeader([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized(new { error = "Authorization header is required" });
            }

            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { error = "Invalid token format. Use 'Bearer {token}'" });
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { error = "Token is empty" });
            }

            var validationResult = _jwtService.ValidateToken(token);

            if (validationResult.IsValid)
            {
                _logger.LogInformation("Token validated successfully for user {Username}", validationResult.Username);
                
                return Ok(new 
                { 
                    message = "Token is valid",
                    username = validationResult.Username,
                    role = validationResult.Role,
                    expiresAt = validationResult.ExpiresAt
                });
            }
            else
            {
                _logger.LogWarning("Token validation failed: {Error}", validationResult.ErrorMessage);
                return Unauthorized(new { error = validationResult.ErrorMessage });
            }
        }
    }
} 