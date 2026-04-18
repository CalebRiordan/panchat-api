using System.ComponentModel.DataAnnotations;

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

    [Required]
    public required string Filename { get; set; }

    public Guid MessageId { get; set; }

    // For future use
    public int? QueueOrder { get; set; }
    
    // Size in bytes
    public long? Size { get; set; }

    // Number of pages (PDF or DOCX)
    public int? PageCount { get; set; }
    
}
