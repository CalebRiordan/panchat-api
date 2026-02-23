using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;

public class MessageDto
{
    [Required]
    public Guid DeviceId { get; set; }

    [StringLength(65536, ErrorMessage = "Content cannot exceed 65536 characters")]
    public string Text { get; set; } = string.Empty;

    [Required]
    public DateTime DateTimeSent { get; set; }

    [JsonIgnore]
    public List<AttachmentDto> Attachments { get; set; } = [];
}
