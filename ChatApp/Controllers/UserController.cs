using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost("registerUser/{userName}/{userId}")]
        public ActionResult<string> GetAllLastMessages(string userName, string userId)
        {
            if (ChatHub.users.Where(u => u.UserId == userId).ToList().Any())
            {
                return Conflict("User already exists");
            }
            ChatHub.users.Add(new User() { UserName = userName, UserId = userId });
            return userId;
        }

        [HttpGet("IsUserIdAvailable/{userId}")]
        public ActionResult<bool> IsUserIdAvailable(string userId)
        {
            if (ChatHub.users.Where(u => u.UserId == userId).ToList().Any())
            {
                return BadRequest(false);
            }
            return true;
        }

        [HttpGet("allUsers")]
        public ActionResult<List<User>> GetAllUsers()
        {
            if (ChatHub.users.Any())
            {
                return BadRequest(ChatHub.users);
            }
            return new List<User>();
        }
    }
}
