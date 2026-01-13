using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;

public class Message
{
    public Guid Id { get; set; }

    [Required]
    public Guid DeviceId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(
        65536,
        MinimumLength = 4,
        ErrorMessage = "Content cannot exceed 65536 characters"
    )]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "ContentType is required")]
    public string ContentType { get; set; } = string.Empty;
    public DateTime DateTimeSent { get; set; } = DateTime.Now;
    public Guid UserId { get; set; }

    // User Navigation Property
    [JsonIgnore]
    public User User { get; set; } = null!;
    public int? QueueOrder { get; set; } = null;
}
