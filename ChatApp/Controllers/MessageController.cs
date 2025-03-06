using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        [HttpGet("allLastMessages/{userName}")]
        public List<Message?> GetAllLastMessages(string userName)
        {
            List<Message?> messages = ChatHub.messages
                .Where(m => m.ReceiverId == userName)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentOn).FirstOrDefault())
                .ToList();
            return messages;
        }

        [HttpGet("inboxMessages/{userName}/{receiverId}")]
        public List<Message> GetInboxMessages(string userName, string receiverId)
        {
            List<Message> messages1 = ChatHub.messages;
            List<Message> messages = ChatHub.messages
                .Where(m => (m.SenderId == userName && m.ReceiverId == receiverId) ||
                             (m.SenderId == receiverId && m.ReceiverId == userName))
                .OrderByDescending(m => m.SentOn) // Order by SentOn to get the latest messages first
                .ToList();
            return messages;
        }
    }
}
