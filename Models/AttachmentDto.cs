using System.ComponentModel.DataAnnotations;

namespace PanChatApi.Models;

public class AttachmentDto
{
    [Required(ErrorMessage = "File is required")]
    public required IFormFile File { get; set; }

    // For future use
    public int? QueueOrder { get; set; }
}