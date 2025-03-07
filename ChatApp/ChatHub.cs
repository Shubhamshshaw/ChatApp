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
        if (users.Count == 0)
        {
            users.Add(new User() { UserName = "Admin", UserId = "Admin", ProfileUrl= "https://dragonball.guru/wp-content/uploads/2021/08/Bardock-Profile-Pic-300x300.png" });
            users.Add(new User() { UserName = "User1", UserId = "User1", ProfileUrl = "https://images.hindustantimes.com/img/2022/08/27/1600x900/Dragon-Ball-Super-Super-Hero_review_1661603386429_1661603386601_1661603386601.webp" });
            users.Add(new User() { UserName = "User2", UserId = "User2", ProfileUrl = "https://w0.peakpx.com/wallpaper/292/79/HD-wallpaper-dragon-ball-dragon-ball-super-super-hero.jpg" });
            users.Add(new User() { UserName = "User3", UserId = "User3", ProfileUrl = "https://w0.peakpx.com/wallpaper/292/79/HD-wallpaper-dragon-ball-dragon-ball-super-super-hero.jpg" });
            users.Add(new User() { UserName = "User4", UserId = "User4", ProfileUrl = "https://pbs.twimg.com/profile_images/1769741269327294464/bwPqFyxG_400x400.jpg" });
        }

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
                SentOn = DateTime.Now - TimeSpan.FromDays(365)
            });
            messages.Add(new Message()
            {
                SenderId = "Admin",
                ReceiverId = "User4",
                ReceiverType = ReceiverType.individual,
                Content = "Hi User1, I am Admin.",
                TenantId = Guid.NewGuid(),
                MessageType = MessageType.normal,
                Reactions = new List<Reaction>(),
                ReceivedBy = new List<Guid>(),
                SeenBy = new List<Guid>(),
                Attachments = new List<Attachment>(),
                SentOn = DateTime.Now - TimeSpan.FromDays(2)
            });
            messages.Add(new Message()
            {
                SenderId = "Admin",
                ReceiverId = "User3",
                ReceiverType = ReceiverType.individual,
                Content = "Hi User1, I am Admin.",
                TenantId = Guid.NewGuid(),
                MessageType = MessageType.normal,
                Reactions = new List<Reaction>(),
                ReceivedBy = new List<Guid>(),
                SeenBy = new List<Guid>(),
                Attachments = new List<Attachment>(),
                SentOn = DateTime.Now - TimeSpan.FromHours(5)
            });
            messages.Add(new Message()
            {
                SenderId = "Admin",
                ReceiverId = "User2",
                ReceiverType = ReceiverType.individual,
                Content = "Hi User1, I am Admin.",
                TenantId = Guid.NewGuid(),
                MessageType = MessageType.normal,
                Reactions = new List<Reaction>(),
                ReceivedBy = new List<Guid>(),
                SeenBy = new List<Guid>(),
                Attachments = new List<Attachment>(),
                SentOn = DateTime.Now - TimeSpan.FromMinutes(10)
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
                SentOn = DateTime.Now
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
        var userConnections = users.Where(u => u.UserId == userId).Select(u => u.ConnectionIdList).FirstOrDefault() ?? new List<string>();
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

public class ProfilePictures
{
    public static Dictionary<int, string> ProfilePicturesList { get; set; } = new Dictionary<int, string>();

    public ProfilePictures()
    {
        ProfilePicturesList.Add(0, "https://pbs.twimg.com/profile_images/1769741269327294464/bwPqFyxG_400x400.jpg");
        ProfilePicturesList.Add(1, "https://w0.peakpx.com/wallpaper/292/79/HD-wallpaper-dragon-ball-dragon-ball-super-super-hero.jpg");
        ProfilePicturesList.Add(2, "https://images.hindustantimes.com/img/2022/08/27/1600x900/Dragon-Ball-Super-Super-Hero_review_1661603386429_1661603386601_1661603386601.webp");
        ProfilePicturesList.Add(3, "https://dragonball.guru/wp-content/uploads/2021/08/Bardock-Profile-Pic-300x300.png");
        ProfilePicturesList.Add(4, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTuH3PkcC9bIjkqI3KT-Wac0klsNwE8QTvHIizZCu0HiUS69ZpweeAdVyl2AMXFlEj_w1Q&usqp=CAU");
        ProfilePicturesList.Add(5, "https://i.pinimg.com/236x/b0/77/be/b077befe8784963c3932cfb1333028e3.jpg");
        ProfilePicturesList.Add(6, "https://wallpapersok.com/images/high/dragon-ball-z-son-goku-saiyan-anime-profile-xpxqh1qo6rri8ugf.jpg");
        ProfilePicturesList.Add(7, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTt5LAnvC1XNNz8ccImoIPNUtk3LQOYL1Q_eQ&s");
        ProfilePicturesList.Add(8, "https://www.writeups.org/wp-content/uploads/Sun-Boy-Pre-Crisis-DC-Comics-LSH-Legion-Super-Heroes.jpg");
        ProfilePicturesList.Add(9, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRwQfP_C4cvMevz5yMCHK1l49m6gmPGNzzsO2CjnTr9l2os0RN12cQpDtx5kZj0me3voQs&usqp=CAU");
        ProfilePicturesList.Add(10, "https://www.pngfind.com/pngs/m/271-2718089_official-profile-arts-super-hero-high-dc-super.png");
        ProfilePicturesList.Add(11, "https://www.kindpng.com/picc/m/68-682805_character-profile-series-thor-marvel-super-heroes-clipart.png");
        ProfilePicturesList.Add(12, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTbFS62Onp0H_YhsrfOYcb12fb1UuKCLK3Z7ycDQjkk7e1aZZ_yzp-TB6M&s");
        ProfilePicturesList.Add(13, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTm70aBvImmRpuj9WYebg71uL_kfw7ieTmbbnUxuabaeQa58kaYC1YSTuI&s");
        ProfilePicturesList.Add(14, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQDJn7MbgjpzhilxP1278p2DQPNNveLYAb2mYTDhaXjNRs6V4B6G54kteU&s");
        ProfilePicturesList.Add(15, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSRJz_Scp5gEpOpcgHhKDfPdkCdyowH_jBOAp__2WhH2oHpNAny3nvJOqY&s");
        ProfilePicturesList.Add(16, "data:image/jpeg;base64,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");
        ProfilePicturesList.Add(17, "data:image/jpeg;base64,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");
        ProfilePicturesList.Add(18, "data:image/jpeg;base64,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");
    }

}