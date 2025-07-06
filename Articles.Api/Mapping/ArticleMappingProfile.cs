using AutoMapper;
using Core.Domain.Models;
using Shared.DTOs;

namespace Articles.Api.Mapping
{
    public class ArticleMappingProfile : Profile
    {
        public ArticleMappingProfile()
        {
            CreateMap<Article, ArticleDto>();
            CreateMap<Article, ArticleSummaryDto>();
            CreateMap<CreateArticleDto, Article>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0));
            CreateMap<UpdateArticleDto, Article>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
} 