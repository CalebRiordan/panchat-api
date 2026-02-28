using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class AttachmentDto
{
    [Required(ErrorMessage = "File is required")]
    public required IFormFile File { get; set; }

    [Required(ErrorMessage ="Date and time message was sent required")]
    public DateTime DateTimeSent { get; set; } = DateTime.Now;

    // For future use
    public int? QueueOrder { get; set; }
}