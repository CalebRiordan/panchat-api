using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;

public enum AttachmentType
{
    Image,
    Pdf,
    Word,
}

public class Attachment
{
    public Guid Id { get; set; }

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string Type { get; set; }

    public Guid MessageId { get; set; }

    // For future use
    public int? QueueOrder { get; set; }
    
}
