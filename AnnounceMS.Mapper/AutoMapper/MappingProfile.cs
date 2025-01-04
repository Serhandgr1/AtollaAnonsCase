using AnnounceMS.Application.DTOs;
using AnnounceMS.Entities.Entities;
using AutoMapper;

namespace AnnounceMS.Mapper.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Announcement, AnnouncementDTO>().ReverseMap();
        }
    }
}
