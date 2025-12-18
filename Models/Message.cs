
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PanChatApi.Models;

public class Message
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Content is required")]
    [StringLength(16, MinimumLength = 4, ErrorMessage = "Content cannot exceed 65536 characters")]
    public string Content { get; set; } = string.Empty;
    [Required(ErrorMessage = "ContentType is required")]
    public string ContentType { get; set; } = string.Empty;
    public DateTime DateTimeSent { get; set; } = DateTime.Now;
    public Guid UserId { get; set; }
    // User Navigation Property
    public IdentityUser User { get; set; } = null!;
}