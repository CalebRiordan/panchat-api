using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;

public enum MessageMediaType
{
    Image,
    Pdf,
}

public enum MessageType
{
    Text,
    Image,
    Pdf,
}

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

    [Required(ErrorMessage = "Type is required")]
    public MessageType Type { get; set; } = MessageType.Text;

    [Required(ErrorMessage = "ContentType is required")]
    public DateTime DateTimeSent { get; set; } = DateTime.Now;
    public Guid UserId { get; set; }

    // User Navigation Property
    [JsonIgnore]
    public User User { get; set; } = null!;
    public string? FileName { get; set; } // Optional: helpful for PDFs
    public long? FileSize { get; set; } // Optional
    public string? Caption { get; set; }

    // For future use
    public int? QueueOrder { get; set; } = null;
}
