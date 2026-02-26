using Microsoft.Win32.SafeHandles;
using Supabase;

namespace PanChatApi.Services;

class FileStorageService(Client supabase) : IFileStorageService
{
    public async Task DeleteFileAsync(string path, string bucketName)
    {
        await supabase.Storage.From(bucketName).Remove(path);
    }

    public async Task<string> GetUrlAsync(string path, string bucketName, int expiresIn = 3600)
    {
        return await supabase.Storage.From(bucketName).CreateSignedUrl(path, expiresIn);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string bucketName)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is null or empty");
        }

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        string ext = Path.GetExtension(file.FileName);
        string fileName = $"{Guid.NewGuid()}{ext}";

        await supabase
            .Storage.From(bucketName)
            .Upload(
                fileBytes,
                fileName,
                new Supabase.Storage.FileOptions { ContentType = file.ContentType }
            );

        return fileName;
    }
}
