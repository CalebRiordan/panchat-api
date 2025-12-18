using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string HashedPassword { get; set; } = string.Empty;
}