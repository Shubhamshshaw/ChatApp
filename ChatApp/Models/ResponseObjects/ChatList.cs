namespace ChatApp.Models.ResponseObjects
{
    public class ChatList
    {
        public string MessageId { get; set; }
        public string ChatId { get; set; }
        public string ChatName { get; set; }
        public string LastMessage { get; set; }
        public string TimeStamp { get; set; }
        public ActiveStatus ActiveStatus { get; set; }
        public Boolean isPinned { get; set; }
        public string ProfileURL { get; set; }
        public Boolean IsLastMsgSeen { get; set; }
    }

    public enum ActiveStatus{
        Offline,
        Available,
        Busy,
        DoNotDisturb,
        Away
    }
}
