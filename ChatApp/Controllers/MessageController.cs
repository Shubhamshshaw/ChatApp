using AutoMapper;
using ChatApp.Models;
using ChatApp.Models.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Linq;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMapper _mapper;
        public MessageController(IMapper mapper)
        {
            _mapper = mapper;
        }
        [HttpGet("allLastChats/{userId}")]
        public List<ChatList?> GetAllLastMessages(string userId)
        {
            new ChatHub();
            List<Message?> messages = ChatHub.messages
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault())
                .ToList();
            var chatLists = _mapper.Map<List<ChatList>>(messages);

            foreach (var message in chatLists)
            {
                var chatId = GetChatId(message, userId).Result;
                message.ChatName = ChatHub.users.FirstOrDefault(u => u.UserId == message.ChatId)?.UserName ?? "Unknown User";
                message.ProfileURL = GetProfileURL(chatId).Result;
                message.ChatId = chatId;
                message.ActiveStatus = GetActiveStatus(chatId, new ChatHub()).Result;
                message.IsPinned = IsChatPinned(userId).Result;
                message.ProfileURL = GetProfileURL(chatId).Result;
            }

            return chatLists
                .GroupBy(m => m.ChatId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault()).ToList<ChatList?>();
        }

        [HttpGet("dm/{userId}/{receiverId}")]
        public ChatboxMessages GetInboxMessages(string userId, string receiverId)
        {
            List<Message> messages = ChatHub.messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId) ||
                             (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentOn) // Order by SentOn to get the latest messages last
                .ToList();

            var receiver = ChatHub.users.FirstOrDefault(u => u.UserId == receiverId);
            ChatboxMessages chatboxMessages = new ChatboxMessages()
            {
                ChatId = receiverId,
                ChatName = receiver?.UserName ?? "Unknown User",
                ActiveStatus = GetActiveStatus(receiverId, new ChatHub()).Result,
                ProfileURL = receiver?.ProfileUrl ?? "Unknown Profile URL",
                ChatMessages = messages
            };

            return chatboxMessages;
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

        private async Task<string> GetProfileURL(string chatId)
        {
            return ChatHub.users.FirstOrDefault(u => u.UserId == chatId)?.ProfileUrl ?? "Unknown Profile URL";
        }

        private async Task<string> GetChatId(ChatList src, string userId)
        {
            return src.SenderId == userId ? src.ReceiverId : src.SenderId;
        }

        private async Task<bool> IsChatPinned(string chatId)
        {
            var user = ChatHub.users.FirstOrDefault(u => u.UserId == chatId);
            if (user is not null)
            {
                var pinnedChats = user.PinnedChatIdList;
                if (pinnedChats is not null)
                    return pinnedChats.Contains(chatId);
            }
            return false;
        }
    }
}
