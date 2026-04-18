using System.IO.Compression;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Data;
using PanChatApi.Models;
using PanChatApi.Services;

namespace PanChatApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class MessageController(
    AppDbContext context,
    IHubContext<PanChatHub, IChatClient> hubContext,
    IFileStorageService storageService,
    ILogger<MessageController> logger
) : ControllerBase
{
    [HttpGet("/api/messages")]
    public async Task<ActionResult<List<Message>>> Get(
        [FromQuery] DateTime? cursorDate,
        [FromQuery] Guid? cursorId,
        [FromQuery] int limit = 50
    )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        // Set up query (don't execute yet)
        var query = context.Messages.AsNoTracking().Where(m => m.UserId == Guid.Parse(userId));

        if (cursorDate.HasValue && cursorId.HasValue)
        {
            // Retrieve messages according to cursor date and ID
            query = query.Where(m =>
                m.DateTimeSent < cursorDate
                || (m.DateTimeSent == cursorDate && m.Id.CompareTo(cursorId) < 0)
            );
        }

        // Execute query
        var messages = await query
            .Include(m => m.Attachments)
            .OrderBy(m => m.DateTimeSent)
            .ThenByDescending(m => m.Id) // Ensures IDs less than the ID of the last message in this page are included in the response
            .Take(limit)
            .ToListAsync();

        if (messages.Count > 0)
        {
            var allPaths = messages.SelectMany(m => m.Attachments.Select(a => a.Url)).ToList();

            logger.LogInformation("\n{@allPaths}\n", allPaths);

            if (allPaths.Count > 0)
            {
                // Get signed URLs from Supabase
                var signedAttachments = await storageService.GetUrlsAsync(
                    allPaths,
                    IFileStorageService.BucketName
                );

                // Update message entities with signed URL
                foreach (var msg in messages)
                {
                    foreach (var att in msg.Attachments)
                    {
                        att.Url =
                            signedAttachments
                                .FirstOrDefault(sa => sa.FilePath == att.Url)
                                ?.SignedUrl ?? "";
                    }
                }
            }
        }

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromForm] MessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return BadRequest();

        var message = new Message
        {
            DeviceId = dto.DeviceId,
            Text = dto.Text,
            DateTimeSent = dto.DateTimeSent,
            UserId = Guid.Parse(userId),
        };

        // Upload files and save file path. Also capture size and page count for PDFs/DOCX.
        List<string> filePaths = [];
        foreach (var att in dto.Attachments)
        {
            // Read file into memory to compute metadata (size, page count)
            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await att.File.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            long? size = fileBytes.LongLength;
            int? pageCount = null;

            var ext = Path.GetExtension(att.File.FileName).ToLowerInvariant();
            if (ext == ".pdf")
            {
                pageCount = GetPdfPageCount(fileBytes);
            }
            else if (ext == ".docx")
            {
                pageCount = GetDocxPageCount(fileBytes);
            }

            var filePath = await storageService.UploadFileAsync(
                att.File,
                IFileStorageService.BucketName
            );
            filePaths.Add(filePath);

            message.Attachments.Add(
                new Attachment
                {
                    Url = filePath,
                    Filename = att.File.FileName,
                    QueueOrder = att.QueueOrder,
                    Type = att.File.ContentType,
                    Size = size,
                    PageCount = pageCount,
                }
            );
        }

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // Get signed URLs for message's attachments
        if (message.Attachments.Count > 0)
        {
            var signedAttachments = await storageService.GetUrlsAsync(
                filePaths,
                IFileStorageService.BucketName
            );

            // Update message entity with signed URL
            foreach (var att in message.Attachments)
            {
                att.Url =
                    signedAttachments.FirstOrDefault(sa => sa.FilePath == att.Url)?.SignedUrl ?? "";
            }
        }

        await hubContext.Clients.User(userId).PushMessage(message);

        return Ok(message);
    }

    private static int? GetDocxPageCount(byte[] fileBytes)
    {
        try
        {
            using var ms = new MemoryStream(fileBytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: true);
            var entry = zip.GetEntry("docProps/app.xml");
            if (entry == null)
                return null;
            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            var pagesElement = doc
                .Root?.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Pages");
            if (pagesElement == null)
                return null;
            if (int.TryParse(pagesElement.Value, out var pages))
                return pages;
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static int? GetPdfPageCount(byte[] fileBytes)
    {
        try
        {
            // Heuristic: count occurrences of "/Type /Page" which is commonly present for each page
            var text = System.Text.Encoding.ASCII.GetString(fileBytes);
            var matches = TypePageRegex().Matches(text);
            if (matches.Count > 0)
                return matches.Count;

            // Fallback: try to locate "/Count <n>" inside a Pages object
            var m = CountNRegex().Match(text);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var cnt))
                return cnt;
            return null;
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"/Type\s*/Page")]
    private static partial Regex TypePageRegex();

    [GeneratedRegex(@"/Count\s+(\d+)")]
    private static partial Regex CountNRegex();
}
