using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;

public class ChatHub : Hub
{
    public static int counter = 0;
    public static List<User> users = new List<User>();
    public static List<Message> messages = new List<Message>();

    public ChatHub()
    {
        users.Clear();
        users.Add(new User() { UserName = "Admin", UserId = "Admin" });
        users.Add(new User() { UserName = "User1", UserId = "User1" });
        if (messages.Count == 0)
        {
            messages.Add(new Message()
            {
                SenderId = "Admin",
                ReceiverId = "User1",
                ReceiverType = ReceiverType.individual,
                Content = "Hi User1, I am Admin.",
                TenantId = Guid.NewGuid(),
                MessageType = MessageType.normal,
                Reactions = new List<Reaction>(),
                ReceivedBy = new List<Guid>(),
                SeenBy = new List<Guid>(),
                Attachments = new List<Attachment>(),
            });
            messages.Add(new Message()
            {
                SenderId = "User1",
                ReceiverId = "Admin",
                ReceiverType = ReceiverType.individual,
                Content = "Hey Admin, How are you?",
                TenantId = Guid.NewGuid(),
                MessageType = MessageType.normal,
                Reactions = new List<Reaction>(),
                ReceivedBy = new List<Guid>(),
                SeenBy = new List<Guid>(),
                Attachments = new List<Attachment>(),
            });
        }
    }

    public async Task SendMessage(string senderId, string userId, string message)
    {
        var receiverLists = users.Where(s => s.UserId == userId || s.UserId == senderId).Select(s => s.ConnectionIdList);
        var receivers = new List<string>();
        foreach (var receiver in receiverLists)
        {
            receivers.AddRange(receiver);
        }
        await Clients.Clients(receivers).SendAsync("ReceiveMessage", senderId, message);
        messages.Add(new Message()
        {
            SenderId = senderId,
            ReceiverId = userId,
            ReceiverType = ReceiverType.individual,
            Content = message,
            TenantId = Guid.NewGuid(),
            MessageType = MessageType.normal,
            Reactions = new List<Reaction>(),
            ReceivedBy = new List<Guid>(),
            SeenBy = new List<Guid>(),
            Attachments = new List<Attachment>(),
        });
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.Request.Query["userid"].ToString();

        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("User Id is missing");
        }

        await Clients.Caller.SendAsync("UserId", userId);
        //await Clients.Caller.SendAsync("Messages", messages);
        //var user = Context.User;
        var connectedUser = users.Where(s => s.UserId.ToLower().Equals(userId.ToLower())).FirstOrDefault();
        if (connectedUser is not null)
        {
            connectedUser.ConnectionIdList.Add(Context.ConnectionId);
            await Clients.Caller.SendAsync("login", Context.ConnectionId);
        }
        else
        {
            await Clients.Caller.SendAsync("login", "failed");
            throw new Exception("User Missing");
        }


        // Track online users
        await base.OnConnectedAsync();
        Console.WriteLine("new connection, total: " + ++counter);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Track offline users
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine("lost connection, total: " + --counter);
    }

    public async Task<bool> CheckActiveStatus(string userId)
    {
        await RemoveZombieConnections(userId);
        if(users.Any(u => u.UserId == userId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private async Task RemoveZombieConnections(string userId)
    {
        // Implement your logic to check if the connection is "zombie"
        // For example, you can ping the connection or check for the last active time
        var userConnections = users.Where(u => u.UserId == userId).Select(u => u.ConnectionIdList).FirstOrDefault();
        foreach (var connectionId in userConnections)
        {
            await PingConnection(connectionId, userId);
        }
    }
    private async Task PingConnection(string connectionId, string userId)
    {
        try
        {
            await Clients.Client(connectionId).SendAsync("Ping");
        }
        catch (Exception ex)
        {
            // Log exception or handle failed ping
            // If ping fails, mark connection as zombie
            var user = users.FirstOrDefault(u => u.UserId == userId);
            user.ConnectionIdList.Remove(connectionId);
        }
    }
}