using Microsoft.AspNetCore.SignalR;
public class ChatHub : Hub
{
    public async Task SendMessage(string senderId, string receiverId, string message)
    {
        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
    }

    public override async Task OnConnectedAsync()
    {
        // Track online users
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Track offline users
        await base.OnDisconnectedAsync(exception);
    }
}