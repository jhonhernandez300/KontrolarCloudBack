using AutoMapper;
using Core.Models;
using Core.DTOs;
using Core;
using System.Data;

namespace KontrolarCloud.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            // Map ProfileDTO to Profile entity
            CreateMap<ProfileDTO, Core.Models.Profile>()
                .ForMember(dest => dest.UsersProfiles, opt => opt.Ignore())
                .ForMember(dest => dest.OptionsProfiles, opt => opt.Ignore());

            // Map the result of the stored procedure to OptionProfileDTO
            CreateMap<IDataReader, OptionProfileDTO>()
                .ForMember(dest => dest.IdModule, opt => opt.MapFrom(src => src["IdModule"]))
                .ForMember(dest => dest.IdOption, opt => opt.MapFrom(src => src["IdOption"]))
                .ForMember(dest => dest.IconOption, opt => opt.MapFrom(src => src["IconOption"]))
                .ForMember(dest => dest.NameOption, opt => opt.MapFrom(src => src["NameOption"]))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src["Description"]))
                .ForMember(dest => dest.Controller, opt => opt.MapFrom(src => src["Controller"]))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src["Action"]))
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => src["OrderBy"]))
                .ForMember(dest => dest.UserAssigned, opt => opt.MapFrom(src => src["UserAssigned"]));

        }
    }
}
