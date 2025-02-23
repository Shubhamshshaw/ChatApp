using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
public class ChatHub : Hub
{
    static int counter = 0;
    static List<User> users = new List<User>();
    public async Task SendMessage(string senderId, string receiverId, string message)
    {
        var receiver = users.Where(s=>s.UserName == receiverId).FirstOrDefault().ConnectionId;
        await Clients.Client(receiver).SendAsync("ReceiveMessage", senderId, message);
    }

    public override async Task OnConnectedAsync()
    {
        // Track online users
        await base.OnConnectedAsync();
        var user = Context.User;
        Console.WriteLine("new connection, total: "+ ++counter);
        users.Add(new User() { ConnectionId = Context.ConnectionId, UserId = Context.UserIdentifier, UserName = $"User {users.Count+1}"});
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Track offline users
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine("lost connection, total: " + --counter);
    }
}