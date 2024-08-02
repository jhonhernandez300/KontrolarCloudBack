using AutoMapper;
using Core.Models;
using KontrolarCloud.DTOs;

namespace KontrolarCloud.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap< Core.Models.Profile, ProfileDTO>().ReverseMap();
        }
    }
}
