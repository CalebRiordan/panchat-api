using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class MediaUploadDto
{
    public Guid DeviceId { get; set; }

    [Required(ErrorMessage = "File is required")]
    public required IFormFile File { get; set; }

    [Required(ErrorMessage = "ContentType is required")]
    public MessageMediaType Type { get; set; } = MessageMediaType.Image;
    public DateTime DateTimeSent { get; set; } = DateTime.Now;
    public string? Caption { get; set; }

    // For future use
    public int? QueueOrder { get; set; }
}
