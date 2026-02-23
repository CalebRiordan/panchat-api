using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PanChatApi.Models;


public class Message
{
    public Guid Id { get; set; }

    [Required]
    public Guid DeviceId { get; set; }

    [Required]
    [StringLength(65536)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public DateTime DateTimeSent { get; set; } = DateTime.Now;

    public Guid UserId { get; set; }

    // User Navigation Property
    [JsonIgnore]
    public User User { get; set; } = null!;

    [JsonIgnore]
    public List<Attachment> Attachments { get; set; } = [];
}
