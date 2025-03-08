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
            Random random = new Random();
            if (ChatHub.users.Where(u => u.UserId == userId).ToList().Any())
            {
                return userId;
            }
            new ProfilePictures();
            ChatHub.users.Add(new User() { UserName = userName, UserId = userId, ProfileUrl = ProfilePictures.ProfilePicturesList[random.Next(0,19)] });
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
                return Ok(ChatHub.users);
            }
            return new List<User>();
        }

        [HttpGet("users/search/{userName}")]
        public ActionResult<List<User>> GetUsersOnSearch(string userName)
        {
            if (ChatHub.users.Where(u => u.UserName.ToLower().Contains(userName.ToLower())).Any())
            {
                return Ok(ChatHub.users);
            }
            return new List<User>();
        }
    }
}
