using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        [HttpGet("allLastMessages/{userId}")]
        public List<Message?> GetAllLastMessages(string userId)
        {
            List<Message?> messages = ChatHub.messages
                .Where(m => m.ReceiverId == userId)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault())
                .ToList();
            return messages;
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
