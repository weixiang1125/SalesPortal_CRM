using AutoMapper;
using SharedLibrary.DTOs;
using SharedLibrary.Models;

namespace CRM_API.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.CreatedByUsername, opt => opt.MapFrom(src => src.CreatedByUser.Username))
                .ForMember(dest => dest.UpdatedByUsername, opt => opt.MapFrom(src => src.UpdatedByUser.Username));

            CreateMap<Deal, DealDto>()
                .ForMember(dest => dest.CreatedByUsername, opt => opt.MapFrom(src => src.CreatedByUser.Username))
                .ForMember(dest => dest.UpdatedByUsername, opt => opt.MapFrom(src => src.UpdatedByUser.Username))
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.Contact.Name));

        }
    }
}
