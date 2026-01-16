using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Data;
using PanChatApi.Models;

namespace PanChatApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(
    AppDbContext context,
    IHubContext<PanChatHub, IChatClient> hubContext
) : ControllerBase
{
    [HttpGet("/api/users/{userId}/messages")]
    public async Task<List<Message>> Get(
        Guid userId,
        [FromQuery] DateTime? cursorDate,
        [FromQuery] Guid? cursorId,
        [FromQuery] int limit = 50
    )
    {
        var query = context.Messages.AsNoTracking().Where(m => m.UserId == userId);

        if (cursorDate.HasValue && cursorId.HasValue)
        {
            query = query.Where(m =>
                m.DateTimeSent < cursorDate
                || (m.DateTimeSent == cursorDate && m.Id.CompareTo(cursorId) < 0)
            );
        }

        return await query
            .OrderByDescending(m => m.DateTimeSent)
            .ThenByDescending(m => m.Id) // Ensures IDs less than the ID of the last message in this page are included in the response
            .Take(limit)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var message = new Message
        {
            DeviceId = dto.DeviceId,
            Content = dto.Content,
            DateTimeSent = dto.DateTimeSent,
            UserId = Guid.Parse(userId),
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        await hubContext.Clients.User(userId).ReceiveMessage(message);

        return Ok(message);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> SendMediaMessage([FromForm] MediaUploadDto dto)
    {
        throw new NotImplementedException();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var allowedExtensions = new[] { ".jpg", ".png", "webp", ".pdf" };
        var extension = Path.GetExtension(dto.File.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file type.");

        // Upload file to storage and get URL
        // ========== TODO: Set up some storage service ===========
        // string fileUrl = await _storageService.UploadAsync(dto.File);
        string fileUrl = "TODO";

        // Create message entity and save to DB
        var message = new Message
        {
            Type = extension == ".pdf" ? MessageType.Pdf : MessageType.Image,
            Content = fileUrl,
            FileName = dto.File.FileName,
            UserId = Guid.Parse(userId),
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // 4. Push via SignalR
        await hubContext.Clients.User(userId).ReceiveMessage(message);

        return Ok(message);
    }
}
