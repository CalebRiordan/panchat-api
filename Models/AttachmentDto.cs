using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class AttachmentDto
{
    [Required(ErrorMessage = "File is required")]
    public required IFormFile File { get; set; }

    [Required(ErrorMessage = "ContentType is required")]
    public AttachmentType Type { get; set; } = AttachmentType.Image;

    public DateTime DateTimeSent { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Attachment must correspond to some Message")]
    public required Message AttachedToMessage { get; set; }

    // For future use
    public int? QueueOrder { get; set; }
}