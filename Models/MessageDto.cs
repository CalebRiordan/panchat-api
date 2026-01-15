using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class MessageDto
{
    [Required]
    public Guid DeviceId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(
        65536,
        MinimumLength = 4,
        ErrorMessage = "Content cannot exceed 65536 characters"
    )]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public DateTime DateTimeSent { get; set; }
}
