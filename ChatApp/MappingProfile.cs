namespace ChatApp;
using AutoMapper;
using ChatApp.Models.ResponseObjects;
using ChatApp.Models;
using System;
using System.Linq;

public class MappingProfile : Profile
{
    IHttpContextAccessor httpContextAccessor;
    public MappingProfile(IHttpContextAccessor _httpContextAccessor)
    {
        this.httpContextAccessor = _httpContextAccessor;
        var chatHub = new ChatHub();
        var userId = this.httpContextAccessor.HttpContext?.Request.Headers["userId"].ToString();
        CreateMap<Message, ChatList>()
            .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ChatName, opt => opt.MapFrom(src => GetUserName(src.ReceiverId)))
            .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => GetChatId(src, userId).Result))
            .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.Content)) // last message is the content of the message
            .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => GetTimeStamp(src.SentOn))) // Customize date formatting if needed
            .ForMember(dest => dest.ActiveStatus, opt => opt.MapFrom(src => GetActiveStatus(userId, chatHub))) // Customize based on user's status
            .ForMember(dest => dest.isPinned, opt => opt.MapFrom(src => IsChatPinned(userId, src))) // Customize based on user's pinned messages
            .ForMember(dest => dest.IsLastMsgSeen, opt => opt.MapFrom(src => src.SeenBy)) // Customize based on seenBy logic
            .ForMember(dest => dest.ProfileURL, opt => opt.MapFrom(src => GetProfileURL(userId, src))); // Customize based on user's profile URL
    }

    private async Task<string> GetProfileURL(string userId, Message src)
    {
        var chatId = await GetChatId(src, userId);
        return ChatHub.users.FirstOrDefault(u => u.UserId == chatId)?.ProfileUrl ?? "Unknown Profile URL";
    }

    private string GetUserName(string receiverId)
    {
        return ChatHub.users.FirstOrDefault(u => u.UserId == receiverId)?.UserName ?? "Unknown User";
    }

    private string GetTimeStamp(DateTime sentOn)
    {
        // If sentOn is DateTime.MinValue, return "Not Set"
        if (sentOn == DateTime.MinValue)
        {
            return "Not Set";
        }

        // Get the current time
        var now = DateTime.Now;

        // Check if the message was sent just now (within the last minute)
        if (now - sentOn < TimeSpan.FromMinutes(1))
        {
            return "Just Now";
        }

        // Check if the message was sent today
        if (sentOn.Date == now.Date)
        {
            return sentOn.ToString("hh:mm tt"); // "01:00 PM"
        }

        // If it was sent on a different day, check if it's from a previous year
        if (sentOn.Year != now.Year)
        {
            return sentOn.ToString("dd-MM-yyyy"); // "01-12-2024"
        }

        // If it was sent on a different day this year, return date in "dd-MM" format
        return sentOn.ToString("dd-MM"); // "01-12"
    }

    private static async Task<ActiveStatus> GetActiveStatus(string userId, ChatHub chatHub)
    {
        // Implement your logic to get the active status of the user
        var result = await chatHub.CheckActiveStatus(userId);
        if (result)
        {
            return ActiveStatus.Available;
        }
        else
        {
            return ActiveStatus.Offline;
        }
    }

    private static async Task<string> GetChatId(Message src, string userId)
    {
        // Check if the userId is available
        if (string.IsNullOrEmpty(userId))
        {
            // Handle case where userId is missing (could log this or handle the error appropriately)
            throw new Exception("UserId header is missing or invalid.");
        }

        // Determine ChatId based on SenderId and ReceiverId
        return src.SenderId == userId ? src.ReceiverId : src.SenderId;
    }

    private async Task<bool> IsChatPinned(string userId, Message src)
    {
        var chatId = await GetChatId(src, userId);
        if (ChatHub.users.FirstOrDefault(u => u.UserId == userId).PinnedChatIdList.Contains(chatId))
        {
            return true;
        }
        return false;
    }
}
