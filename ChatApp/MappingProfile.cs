using AutoMapper;
using Microsoft.AspNetCore.Http;
using ChatApp.Models.ResponseObjects;
using ChatApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Define your mapping configuration
            CreateMap<Message, ChatList>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ChatName, opt => opt.MapFrom((src, dest, member, context) => GetUserName(src.ReceiverId, context)))
                .ForMember(dest => dest.ChatId, opt => opt.MapFrom((src, dest, member, context) => GetChatId(src, context).Result))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => GetTimeStamp(src.SentOn)))
                .ForMember(dest => dest.ActiveStatus, opt => opt.MapFrom((src, dest, member, context) => GetActiveStatus(src.ReceiverId, context).Result))
                .ForMember(dest => dest.isPinned, opt => opt.MapFrom((src, dest, member, context) => IsChatPinned(src, context).Result))
                .ForMember(dest => dest.IsLastMsgSeen, opt => opt.MapFrom(src => src.SeenBy))
                .ForMember(dest => dest.ProfileURL, opt => opt.MapFrom((src, dest, member, context) => GetProfileURL(src, context).Result));
        }

        private string GetUserName(string receiverId, ResolutionContext context)
        {
            // Retrieve IHttpContextAccessor from the context
            var httpContextAccessor = (IHttpContextAccessor)context.Items["IHttpContextAccessor"];
            return httpContextAccessor.HttpContext?.Request.Headers["userId"].ToString() ?? "Unknown User";
        }

        private async Task<string> GetProfileURL(Message src, ResolutionContext context)
        {
            var userId = GetUserIdFromContext(context);
            var chatId = await GetChatId(src, context);
            return ChatHub.users.FirstOrDefault(u => u.UserId == chatId)?.ProfileUrl ?? "Unknown Profile URL";
        }

        private string GetTimeStamp(DateTime sentOn)
        {
            if (sentOn == DateTime.MinValue) return "Not Set";

            var now = DateTime.Now;
            if (now - sentOn < TimeSpan.FromMinutes(1)) return "Just Now";
            if (sentOn.Date == now.Date) return sentOn.ToString("hh:mm tt");

            return sentOn.Year != now.Year ? sentOn.ToString("dd-MM-yyyy") : sentOn.ToString("dd-MM");
        }

        private async Task<string> GetChatId(Message src, ResolutionContext context)
        {
            var userId = GetUserIdFromContext(context);
            return src.SenderId == userId ? src.ReceiverId : src.SenderId;
        }

        private string GetUserIdFromContext(ResolutionContext context)
        {
            // Retrieve IHttpContextAccessor from context
            var httpContextAccessor = (IHttpContextAccessor)context.Items["IHttpContextAccessor"];
            return httpContextAccessor.HttpContext?.Request.Headers["userId"].ToString();
        }

        private async Task<ActiveStatus> GetActiveStatus(string receiverId, ResolutionContext context)
        {
            var userId = GetUserIdFromContext(context);
            var chatHub = new ChatHub();
            var isActive = await chatHub.CheckActiveStatus(userId);
            return isActive ? ActiveStatus.Available : ActiveStatus.Offline;
        }

        private async Task<bool> IsChatPinned(Message src, ResolutionContext context)
        {
            var userId = GetUserIdFromContext(context);
            var chatId = await GetChatId(src, context);
            return ChatHub.users.FirstOrDefault(u => u.UserId == userId)?.PinnedChatIdList.Contains(chatId) ?? false;
        }
    }
}
