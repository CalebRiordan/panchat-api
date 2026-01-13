using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PanChatApi.Data;
using PanChatApi.Models;

namespace PanChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController(AppDbContext context, IConfiguration config) : ControllerBase
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
}
