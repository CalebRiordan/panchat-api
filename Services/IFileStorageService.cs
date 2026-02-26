interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string bucketName);
    Task<string> GetUrlAsync(string path, string bucketName, int expiresIn = 3600);
    Task DeleteFileAsync(string path, string bucketName);
}