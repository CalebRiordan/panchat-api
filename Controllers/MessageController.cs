using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
public class MessageController(
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

        // Upload files and save file path
        List<string> filePaths = [];
        foreach (var att in dto.Attachments)
        {
            var filePath = await storageService.UploadFileAsync(
                att.File,
                IFileStorageService.BucketName
            );
            filePaths.Add(filePath);

            message.Attachments.Add(new Attachment { Url = filePath, QueueOrder = att.QueueOrder, Type = att.File.ContentType });
        }

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // logger.LogInformation("\n\nSaved message: {@message}\n\n", message);

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
}
