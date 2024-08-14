using AutoMapper;
using Core.Models;
using KontrolarCloud.DTOs;
using Core;

namespace KontrolarCloud.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<ProfileDTO, Core.Models.Profile>()
            .ForMember(dest => dest.UsersProfiles, opt => opt.Ignore())
            .ForMember(dest => dest.OptionsProfiles, opt => opt.Ignore());            
        }
    }
}
