using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Shared.DTOs;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Core.Services
{


    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly bool _useS3;
        private readonly IS3ArticleProvider _s3ArticleProvider;

        public ArticleService(
            IArticleRepository articleRepository, 
            IConfiguration configuration,
            IMapper mapper,
            IMemoryCache cache,
            IS3ArticleProvider s3ArticleProvider)
        {
            _articleRepository = articleRepository;
            _configuration = configuration;
            _mapper = mapper;
            _cache = cache;
            _useS3 = bool.TryParse(_configuration["UseS3"], out bool useS3) && useS3;
            _s3ArticleProvider = s3ArticleProvider;
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return _useS3 
                ? await _s3ArticleProvider.GetArticleByIdAsync(id)
                : await _articleRepository.GetByIdAsync(id);
        }

        public async Task<Article?> GetArticleByTitleAsync(string title)
        {
            return _useS3 
                ? await _s3ArticleProvider.GetArticleByTitleAsync(title)
                : await _articleRepository.GetByTitleAsync(title);
        }

        public async Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3)
            {
                return await GetFilteredArticlesFromS3Async(a => true, parameters);
            }

            var allArticles = await _articleRepository.GetAllAsync();
            var totalCount = await _articleRepository.GetTotalCountAsync();
            return CreatePaginationResult(allArticles, totalCount, parameters);
        }

        // Helper method to validate pagination parameters
        private void ValidatePaginationParameters(PaginationParameters parameters)
        {
            if (parameters.PageNumber < 1)
                parameters.PageNumber = 1;
                
            if (parameters.PageSize < 1)
                parameters.PageSize = 10;
                
            if (parameters.PageSize > 50)
                parameters.PageSize = 50;
        }

        // Helper method to create pagination result from a list of articles
        private PaginationResult<Article> CreatePaginationResult(IEnumerable<Article> articles, int totalCount, PaginationParameters parameters)
        {
            var pagedArticles = articles
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            return new PaginationResult<Article>
            {
                Items = pagedArticles,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
            };
        }

        // Generic method to handle S3 filtering and pagination
        private async Task<PaginationResult<Article>> GetFilteredArticlesFromS3Async(
            Func<Article, bool> filterPredicate, 
            PaginationParameters parameters)
        {
            try
            {
                var allArticles = await _s3ArticleProvider.GetAllArticlesAsync();
                var filteredArticles = allArticles.Where(filterPredicate).ToList();
                return CreatePaginationResult(filteredArticles, filteredArticles.Count, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"S3 service is currently unavailable: {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetArticlesByTagAsync(string tag, PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3)
            {
                return await GetFilteredArticlesFromS3Async(
                    a => a.Tags?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true, 
                    parameters);
            }

            var articlesByTag = await _articleRepository.GetByTagAsync(tag);
            var totalCount = await _articleRepository.GetByTagCountAsync(tag);
            return CreatePaginationResult(articlesByTag, totalCount, parameters);
        }

        public async Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3)
            {
                return await GetFilteredArticlesFromS3Async(a => a.IsPublished, parameters);
            }

            var publishedArticles = await _articleRepository.GetPublishedAsync();
            var totalCount = await _articleRepository.GetPublishedCountAsync();
            return CreatePaginationResult(publishedArticles, totalCount, parameters);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            if (await _articleRepository.ExistsByTitleAsync(article.Title))
            {
                throw new InvalidOperationException($"Article with title '{article.Title}' already exists.");
            }

            article.CreatedAt = DateTime.UtcNow;
            article.ViewCount = 0;

            return await _articleRepository.AddAsync(article);
        }

        public async Task<Article> UpdateArticleAsync(Article article)
        {
            var existingArticle = await _articleRepository.GetByIdAsync(article.Id);
            if (existingArticle == null)
            {
                throw new InvalidOperationException($"Article with ID {article.Id} not found.");
            }

            // Check for title duplication if changed
            if (article.Title != existingArticle.Title && 
                await _articleRepository.ExistsByTitleAsync(article.Title))
            {
                throw new InvalidOperationException($"Article with title '{article.Title}' already exists.");
            }

            article.UpdatedAt = DateTime.UtcNow;
            article.CreatedAt = existingArticle.CreatedAt;
            article.ViewCount = existingArticle.ViewCount;

            return await _articleRepository.UpdateAsync(article);
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            if (!await _articleRepository.ExistsAsync(id))
            {
                throw new InvalidOperationException($"Article with ID {id} not found.");
            }

            return await _articleRepository.DeleteAsync(id);
        }

        public async Task<PaginationResult<Article>> GetArticlesByNewspaperAsync(int newspaperId, PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3)
            {
                return await GetFilteredArticlesFromS3Async(a => a.NewspaperId == newspaperId, parameters);
            }

            var articlesByNewspaper = await _articleRepository.GetByNewspaperAsync(newspaperId);
            var totalCount = await _articleRepository.GetByNewspaperCountAsync(newspaperId);
            return CreatePaginationResult(articlesByNewspaper, totalCount, parameters);
        }
    }
} 