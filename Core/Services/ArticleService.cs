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
using Core.Domain.Exceptions;

namespace Core.Services
{


    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _SqlarticleRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly bool _useS3;
        private readonly IS3ArticleProvider? _s3ArticleRepository;
        private readonly IS3FileProvider? _s3FileProvider;

        public ArticleService(
            IArticleRepository articleRepository, 
            IConfiguration configuration,
            IMapper mapper,
            IMemoryCache cache,
            IS3ArticleProvider? s3ArticleProvider = null,
            IS3FileProvider? s3FileProvider = null)
        {
            _SqlarticleRepository = articleRepository;
            _configuration = configuration;
            _mapper = mapper;
            _cache = cache;
            _useS3 = bool.TryParse(_configuration["StorageSettings:UseS3"], out bool useS3) && useS3;
            _s3ArticleRepository = s3ArticleProvider;
            _s3FileProvider = s3FileProvider;
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return _useS3 && _s3ArticleRepository != null
                ? await _s3ArticleRepository.GetArticleByIdAsync(id)
                : await _SqlarticleRepository.GetByIdAsync(id);
        }

        public async Task<Article?> GetArticleByTitleAsync(string title)
        {
            return _useS3 && _s3ArticleRepository != null
                ? await _s3ArticleRepository.GetArticleByTitleAsync(title)
                : await _SqlarticleRepository.GetByTitleAsync(title);
        }

        public async Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3 && _s3ArticleRepository != null)
            {
                return await GetFilteredArticlesFromS3Async(a => true, parameters);
            }

            return await _SqlarticleRepository.GetAllAsync(parameters);
        }

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

        
        private async Task<PaginationResult<Article>> GetFilteredArticlesFromS3Async(
            Func<Article, bool> filterPredicate, 
            PaginationParameters parameters)
        {
            try
            {
                var allArticles = await _s3ArticleRepository!.GetAllArticlesAsync();
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
            
            if (_useS3 && _s3ArticleRepository != null)
            {
                return await GetFilteredArticlesFromS3Async(
                    a => a.Tags?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true, 
                    parameters);
            }
            return await _SqlarticleRepository.GetByTagAsync(tag, parameters);
        }

        public async Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3 && _s3ArticleRepository != null)
            {
                return await GetFilteredArticlesFromS3Async(a => a.IsPublished, parameters);
            }

            return await _SqlarticleRepository.GetPublishedAsync(parameters);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            if (await _SqlarticleRepository.ExistsByTitleAsync(article.Title))
            {
                throw new DuplicateEntityException("Article", "title", article.Title);
            }

            article.CreatedAt = DateTime.UtcNow;
            article.ViewCount = 0;

            return await _SqlarticleRepository.AddAsync(article);
        }

        public async Task<Article> UpdateArticleAsync(Article article)
        {
            var existingArticle = await _SqlarticleRepository.GetByIdAsync(article.Id);
            if (existingArticle == null)
            {
                throw new ArticleNotFoundException(article.Id);
            }

            // Check for title duplication if changed
            if (article.Title != existingArticle.Title && 
                await _SqlarticleRepository.ExistsByTitleAsync(article.Title))
            {
                throw new DuplicateEntityException("Article", "title", article.Title);
            }

            article.UpdatedAt = DateTime.UtcNow;
            article.CreatedAt = existingArticle.CreatedAt;
            article.ViewCount = existingArticle.ViewCount;

            return await _SqlarticleRepository.UpdateAsync(article);
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            if (!await _SqlarticleRepository.ExistsAsync(id))
            {
                throw new ArticleNotFoundException(id);
            }

            return await _SqlarticleRepository.DeleteAsync(id);
        }

        public async Task<PaginationResult<Article>> GetArticlesByNewspaperAsync(int newspaperId, PaginationParameters parameters)
        {
            ValidatePaginationParameters(parameters);
            
            if (_useS3 && _s3ArticleRepository != null)
            {
                return await GetFilteredArticlesFromS3Async(a => a.NewspaperId == newspaperId, parameters);
            }

            return await _SqlarticleRepository.GetByNewspaperAsync(newspaperId, parameters);
        }


        // ===== S3 File Operations =====

        public async Task UploadFileToS3Async(string filePath, string keyName, string? contentType = null)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            await _s3FileProvider.UploadFileAsync(filePath, keyName, contentType);
        }

        public async Task UploadStreamToS3Async(Stream stream, string keyName, string contentType)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            await _s3FileProvider.UploadStreamAsync(stream, keyName, contentType);
        }

        // Removed unsupported methods - using only interface methods
        

        public async Task<string> GetS3FileContentAsStringAsync(string keyName)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            return await _s3FileProvider.GetFileContentAsStringAsync(keyName);
        }

        public async Task<List<string>> ListS3ObjectsAsync(string? prefix = null, int? maxKeys = null)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            return await _s3FileProvider.ListObjectsAsync(prefix, maxKeys);
        }

        public async Task DeleteS3ObjectAsync(string keyName)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            await _s3FileProvider.DeleteObjectAsync(keyName);
        }

        // Removed - not in interface

        public async Task<bool> S3ObjectExistsAsync(string keyName)
        {
            if (!_useS3 || _s3FileProvider == null)
            {
                throw new InvalidOperationException("S3 is not enabled or S3FileProvider is not available.");
            }

            return await _s3FileProvider.ObjectExistsAsync(keyName);
        }

        // Removed - not in interface
    }
}