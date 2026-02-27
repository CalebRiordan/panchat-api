using PanChatApi.Models;

namespace PanChatApi.Services;

public class SupabaseStorageService(Supabase.Client supabase) : IFileStorageService
{
    public async Task DeleteFileAsync(string path, string bucketName)
    {
        await supabase.Storage.From(bucketName).Remove(path);
    }

    public async Task<string> GetUrlAsync(string path, string bucketName, int expiresIn = 3600)
    {
        return await supabase.Storage.From(bucketName).CreateSignedUrl(path, expiresIn);
    }

    public async Task<List<SignedAttachment>> GetUrlsAsync(List<string> paths, string bucketName, int expiresIn = 3600)
    {
        var responses = await supabase.Storage.From(bucketName).CreateSignedUrls(paths, expiresIn);
        if (responses == null || responses.Count == 0)
        {
            return [];    
        } 
        return responses.Select(s => new SignedAttachment{ FilePath = s.Path!, SignedUrl = s.SignedUrl!}).ToList();
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
        string fileName = $"{Guid.NewGuid()}_{ext}";

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
