using AutoMapper;
using Core.Domain.Models;
using Shared.DTOs;

namespace Articles.Api.Mapping
{
    public class NewspaperMappingProfile : Profile
    {
        public NewspaperMappingProfile()
        {
            CreateMap<Newspaper, NewspaperDto>()
                .ForMember(dest => dest.ArticlesCount, opt => opt.MapFrom(src => src.Articles.Count));
            
            CreateMap<CreateNewspaperDto, Newspaper>();
            CreateMap<UpdateNewspaperDto, Newspaper>();
        }
    }
} 