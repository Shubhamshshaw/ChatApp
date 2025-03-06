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
    }
}
