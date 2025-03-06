using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;
public class ChatHub : Hub
{
    static int counter = 0;
    static List<User> users = new List<User>();
    static List<Message> messages = new List<Message>();

    public ChatHub()
    {
        users.Add(new User() { UserName = "Admin" });
        users.Add(new User() { UserName = "User1" });
        if (messages.Count == 0)
        {
            messages.Add(new Message()
            {
                SenderId = "Admin",
                ReceiverId = "User1",
                ReceiverType = ReceiverType.individual,
                Content = "Hi User2, I am Admin.",
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

    public async Task SendMessage(string senderId, string userName, string message)
    {
        var receiverLists = users.Where(s => s.UserName == userName).Select(s => s.ConnectionIdList);
        var receivers = new List<string>();
        foreach (var receiver in receiverLists)
        {
            receivers.AddRange(receiver);
        }
        await Clients.Clients(receivers).SendAsync("ReceiveMessage", senderId, message);
        messages.Add(new Message()
        {
            SenderId = senderId,
            ReceiverId = userName,
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
        var userName = httpContext?.Request.Query["username"].ToString();

        if (string.IsNullOrEmpty(userName))
        {
            userName = $"User {users.Count + 1}";
        }

        await Clients.Caller.SendAsync("UserName", userName);
        // Track online users
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Messages", messages);
        var user = Context.User;
        Console.WriteLine("new connection, total: " + ++counter);
        var connectedUser = users.Where(s => s.UserName.ToLower().Equals(userName.ToLower())).FirstOrDefault();
        if (connectedUser is not null)
        {
            connectedUser.ConnectionIdList.Add(Context.ConnectionId);
        }
        else
        {
            users.Add(new User()
            {
                UserName = userName,
                UserId = Context.UserIdentifier,
                ConnectionIdList = new List<string> { Context.ConnectionId }
            });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Track offline users
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine("lost connection, total: " + --counter);
    }
}