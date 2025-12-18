using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class UserDto
{
    [Required]
    [StringLength(16, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 16 characters")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(16, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 16 characters")]
    public string Password { get; set; } = string.Empty;
}