using AutoMapper;
using Core.Domain.Models;
using Shared.DTOs;

namespace Articles.Api.Mapping
{

    #region summary
    /// ArticleMappingProfile defines AutoMapper configurations for Article entities.
    /// 
    /// Purpose:
    /// - Maps between Article domain models and DTOs
    /// - Handles complex mapping scenarios with relationships
    /// - Ensures consistent data transformation across the application
    /// - Provides clean separation between domain and API layers
    /// 
    /// Dependencies:
    /// - AutoMapper framework for object mapping
    /// - Core.Domain.Models for source entities
    /// - Shared.DTOs for target objects
    /// - Entity Framework for relationship loading
    /// 
    /// Alternatives:
    /// - Could implement manual mapping for complex scenarios
    /// - Could use different mapping libraries (Mapster, ExpressMapper)
    /// - Could implement custom value resolvers
    /// - Could add mapping validation 
    #endregion

    public class ArticleMappingProfile : Profile
    {
        public ArticleMappingProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(dest => dest.NewspaperId, opt => opt.MapFrom(src => src.NewspaperId))
                .ForMember(dest => dest.NewspaperName, opt => opt.MapFrom(src => src.Newspaper != null ? src.Newspaper.Name : null));
            
            CreateMap<Article, ArticleSummaryDto>();
            CreateMap<CreateArticleDto, Article>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0));
            CreateMap<UpdateArticleDto, Article>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
} 