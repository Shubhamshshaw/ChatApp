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
        [HttpGet("allLastMessages/{userId}")]
        public List<ChatList> GetAllLastMessages(string userId)
        {
            List<Message?> messages = ChatHub.messages
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault())
                .ToList();
            return _mapper.Map<List<ChatList>>(messages);
        }

        [HttpGet("inboxMessages/{userId}/{receiverId}")]
        public List<Message> GetInboxMessages(string userId, string receiverId)
        {
            List<Message> messages1 = ChatHub.messages;
            List<Message> messages = ChatHub.messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId) ||
                             (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentOn) // Order by SentOn to get the latest messages last
                .ToList();
            return messages;
        }
    }
}
