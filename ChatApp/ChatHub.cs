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
        // Track online users
        await base.OnConnectedAsync();
        //await Clients.Caller.SendAsync("Messages", messages);
        var user = Context.User;
        Console.WriteLine("new connection, total: " + ++counter);
        var connectedUser = users.Where(s => s.UserId.ToLower().Equals(userId.ToLower())).FirstOrDefault();
        if (connectedUser is not null)
        {
            connectedUser.ConnectionIdList.Add(Context.ConnectionId);
            await Clients.Caller.SendAsync("login", "Successful");
        }
        else
        {
            await Clients.Caller.SendAsync("login", "failed");
            throw new Exception("User Missing");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Track offline users
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine("lost connection, total: " + --counter);
    }
}