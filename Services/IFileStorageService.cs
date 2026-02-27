using PanChatApi.Models;

namespace PanChatApi.Services;

public interface IFileStorageService
{
    const string BucketName = "attachments";
    Task<string> UploadFileAsync(IFormFile file, string bucketName);
    Task<string> GetUrlAsync(string path, string bucketName, int expiresIn = 3600);
    Task<List<SignedAttachment>> GetUrlsAsync(List<string> paths, string bucketName, int expiresIn = 3600);
    Task DeleteFileAsync(string path, string bucketName);
}