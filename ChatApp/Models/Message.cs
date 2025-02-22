namespace ChatApp.Models;

public class Message
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string ReceiverType { get; set; } // individual/group/channel
    public string Content { get; set; }
    public Guid TenantId { get; set; }
    public string MessageType { get; set; } // normal msg/poll
    public List<Reaction> Reactions { get; set; }
    public List<Guid> ReceivedBy { get; set; }
    public List<Guid> SeenBy { get; set; }
    public List<Attachment> Attachments { get; set; }
}

public class Reaction
{
    public string ReactionType { get; set; }
    public List<Guid> UserIds { get; set; }
}

public class Attachment
{
    public string AttachmentType { get; set; }
    public string AttachmentUrl { get; set; }
}
