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
                .ForMember(dest => dest.ReceiverId, opt => opt.MapFrom(src => src.ReceiverId))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => GetTimeStamp(src.SentOn)))
                .ForMember(dest => dest.IsLastMsgSeen, opt => opt.MapFrom(src => src.SeenBy.Any()));
        }

        private string GetTimeStamp(DateTime sentOn)
        {
            if (sentOn == DateTime.MinValue) return "Not Set";

            var now = DateTime.Now;
            if (now - sentOn < TimeSpan.FromMinutes(1)) return "Just Now";
            if (sentOn.Date == now.Date) return sentOn.ToString("hh:mm tt");

            return sentOn.Year != now.Year ? sentOn.ToString("dd-MM-yyyy") : sentOn.ToString("dd-MM");
        }
    }
}
