using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace Core.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenDto GenerateToken(string username)
        {
            var secretKey = GetJwtSecretKey();
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenDto
            {
                Token = tokenString,
                Username = username,
                ExpiresAt = token.ValidTo
            };
        }

        public bool ValidateCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var usersSection = _configuration.GetSection("Auth:DemoUsers");
            if (!usersSection.Exists())
            {
                return false;
            }

            var userNode = usersSection.GetChildren()
                .FirstOrDefault(x => string.Equals(x["Username"], username, StringComparison.OrdinalIgnoreCase));

            var configuredPassword = userNode?["Password"];
            if (string.IsNullOrEmpty(configuredPassword))
            {
                return false;
            }

            var configuredBytes = Encoding.UTF8.GetBytes(configuredPassword);
            var inputBytes = Encoding.UTF8.GetBytes(password);
            if (configuredBytes.Length != inputBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(configuredBytes, inputBytes);
        }

        public Shared.DTOs.TokenValidationResult ValidateToken(string token)
        {
            try
            {
                var secretKey = GetJwtSecretKey();
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                {
                    return new Shared.DTOs.TokenValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "JWT configuration is missing"
                    };
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                    var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                    var expiration = jwtToken.ValidTo;

                    return new Shared.DTOs.TokenValidationResult
                    {
                        IsValid = true,
                        Username = username,
                        Role = role,
                        ExpiresAt = expiration,
                        ErrorMessage = null
                    };
                }

                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token format"
                };
            }
            catch (SecurityTokenExpiredException)
            {
                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token has expired"
                };
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token signature"
                };
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token issuer"
                };
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token audience"
                };
            }
            catch (Exception)
            {
                return new Shared.DTOs.TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token validation failed"
                };
            }
        }

        private string GetJwtSecretKey()
        {
            var secret = _configuration["JwtSettings:SecretKey"]
                ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }

            return secret;
        }


    }
} 