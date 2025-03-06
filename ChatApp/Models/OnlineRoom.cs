namespace ChatApp.Models;

public class OnlineRoom
{
    public Guid RoomId { get; set; }
    public Guid TenantId { get; set; }
    public List<User> OnlineUsers { get; set; } = new List<User>();
}
