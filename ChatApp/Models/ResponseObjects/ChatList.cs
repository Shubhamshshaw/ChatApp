namespace ChatApp.Models.ResponseObjects
{
    public class ChatList
    {
    }

    public class ChatListResponse
    {
        public string ChatId { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverType { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string Reactions { get; set; }
        public string ReceivedBy { get; set; }
        public string SeenBy { get; set; }
        public string Attachments { get; set; }
    }
}
