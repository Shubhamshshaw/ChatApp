namespace ChatApp.Models;

public class OnlineRoom
{
    public Guid RoomId { get; set; }
    public Guid TenantId { get; set; }
    public List<Guid> OnlineUsers { get; set; }
}
