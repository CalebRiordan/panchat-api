using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;

public enum AttachmentType
{
    Image,
    Pdf,
}

public class Attachment
{
    public Guid Id { get; set; }

    [Required]
    public required string Url { get; set; }

    [Required]
    [MaxLength(5)]
    public AttachmentType Type { get; set; } = AttachmentType.Image;

    public DateTime DateTimeSent { get; set; } = DateTime.Now;

    [Required]
    public Guid MessageId { get; set; }

    [JsonIgnore]
    public Message Message { get; set; } = null!;

    // For future use
    public int? QueueOrder { get; set; }
    
}
