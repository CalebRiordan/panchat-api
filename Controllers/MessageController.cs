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

        logger.LogInformation("Getting messages with cursorId: {@cursor}", cursorId);

        var query = context.Messages.AsNoTracking().Where(m => m.UserId == Guid.Parse(userId));

        if (cursorDate.HasValue && cursorId.HasValue)
        {
            query = query.Where(m =>
                m.DateTimeSent < cursorDate
                || (m.DateTimeSent == cursorDate && m.Id.CompareTo(cursorId) < 0)
            );
        }

        var messages = await query
            .Include(m => m.Attachments)
            .OrderBy(m => m.DateTimeSent)
            .ThenByDescending(m => m.Id) // Ensures IDs less than the ID of the last message in this page are included in the response
            .Take(limit)
            .ToListAsync();

        if (messages.Count == 0)
            return Ok(messages);

        var allPaths = messages.SelectMany(m => m.Attachments.Select(a => a.Url)).ToList();

        if (allPaths.Count > 0)
        {
            var signedAttachments = await storageService.GetUrlsAsync(
                allPaths,
                IFileStorageService.BucketName
            );

            foreach (var msg in messages)
            {
                foreach (var att in msg.Attachments)
                {
                    att.Url =
                        signedAttachments.FirstOrDefault(sa => sa.FilePath == att.Url)?.SignedUrl
                        ?? "";
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
        foreach (var att in dto.Attachments)
        {
            var filePath = await storageService.UploadFileAsync(
                att.File,
                IFileStorageService.BucketName
            );
            message.Attachments.Add(
                new Attachment
                {
                    Url = filePath,
                    QueueOrder = att.QueueOrder,
                }
            );
        }

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // logger.LogInformation("\n\nSaved message: {@message}\n\n", message);
        // TODO: Dont push message with file path as attachment URL. Get signed URLs

        await hubContext.Clients.User(userId).PushMessage(message);

        return Ok(message);
    }
}
