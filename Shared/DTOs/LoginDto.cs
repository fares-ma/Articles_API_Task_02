using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class LoginDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
} 