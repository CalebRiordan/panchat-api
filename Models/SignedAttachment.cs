namespace PanChatApi.Models;

public class SignedAttachment
{
    public required string FilePath {get; set;}
    public required string SignedUrl {get; set;}
}