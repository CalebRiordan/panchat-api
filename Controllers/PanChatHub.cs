using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PanChatApi.Models;

namespace PanChatApi.Controllers;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task UserTyping(Guid userId);
}

[Authorize]
public class PanChatHub : Hub<IChatClient> { }
