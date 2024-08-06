using AutoMapper;
using Core.Models;
using KontrolarCloud.DTOs;
using Core;

namespace KontrolarCloud.Mapping
{
    public class MappingUser : AutoMapper.Profile
    {
        public MappingUser()
        {
            CreateMap<UserDTO, Core.Models.User>()
            .ForMember(dest => dest.UserCompanies, opt => opt.Ignore())
            .ForMember(dest => dest.UsersProfiles, opt => opt.Ignore());
        }
    }
}
