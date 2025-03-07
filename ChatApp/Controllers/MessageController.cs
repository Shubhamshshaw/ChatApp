using AutoMapper;
using ChatApp.Models;
using ChatApp.Models.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        public List<ChatList> GetAllLastMessages(string userId)
        {
            List<Message?> messages = ChatHub.messages
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault())
                .ToList();
            return _mapper.Map<List<ChatList>>(messages);
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
    }
}
