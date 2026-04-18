using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Core.Domain.Exceptions;
using Shared.DTOs;

namespace Core.Services
{
    /// <summary>
    /// S3 implementation of IArticleService
    /// Handles all article operations using Amazon S3 storage with caching
    /// </summary>
    public class S3ArticleService : IArticleService
    {
        private readonly IS3FileProvider _s3FileProvider;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<S3ArticleService> _logger;
        private readonly string _articlesKey;
        private const string S3_ARTICLES_CACHE_KEY = "S3_ALL_ARTICLES";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public S3ArticleService(
            IS3FileProvider s3FileProvider,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<S3ArticleService> logger,
            IConfiguration configuration)
        {
            _s3FileProvider = s3FileProvider ?? throw new ArgumentNullException(nameof(s3FileProvider));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _articlesKey = configuration["AWS:ArticlesKey"] ?? "articles/all-articles.json";
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting article by ID from S3: {ArticleId}", id);
                var articles = await GetAllArticlesFromS3Async();
                var article = articles.FirstOrDefault(a => a.Id == id);
                
                if (article == null)
                {
                    _logger.LogWarning("Article with ID {ArticleId} not found in S3", id);
                }
                
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article by ID from S3: {ArticleId}", id);
                throw new S3ArticleServiceException("GetById", $"Failed to get article by ID {id}: {ex.Message}");
            }
        }

        public async Task<Article?> GetArticleByTitleAsync(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentException("Title cannot be null or empty", nameof(title));
                }

                _logger.LogInformation("Getting article by title from S3: {Title}", title);
                
                // Try to get from S3FileProvider's GetObjectByTitle first
                var content = await _s3FileProvider.GetObjectByTitleAsync(title);
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        var articleDto = JsonConvert.DeserializeObject<ArticleDto>(content);
                        if (articleDto != null)
                        {
                            return _mapper.Map<Article>(articleDto);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize article from S3 object by title: {Title}", title);
                    }
                }

                // Fallback to searching in all articles
                var articles = await GetAllArticlesFromS3Async();
                var article = articles.FirstOrDefault(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                
                if (article == null)
                {
                    _logger.LogWarning("Article with title '{Title}' not found in S3", title);
                }
                
                return article;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article by title from S3: {Title}", title);
                throw new S3ArticleServiceException("GetByTitle", $"Failed to get article by title '{title}': {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting all articles from S3 with pagination: Page {PageNumber}, Size {PageSize}", 
                    parameters.PageNumber, parameters.PageSize);
                
                var allArticles = await GetAllArticlesFromS3Async();
                return CreatePaginationResult(allArticles, allArticles.Count, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all articles from S3");
                throw new S3ArticleServiceException("GetAll", $"Failed to get all articles: {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetArticlesByTagAsync(string tag, PaginationParameters parameters)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    throw new ArgumentException("Tag cannot be null or empty", nameof(tag));
                }

                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting articles by tag from S3: {Tag}, Page {PageNumber}, Size {PageSize}", 
                    tag, parameters.PageNumber, parameters.PageSize);
                
                var allArticles = await GetAllArticlesFromS3Async();
                var filteredArticles = allArticles.Where(a => 
                    a.Tags?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true).ToList();
                
                return CreatePaginationResult(filteredArticles, filteredArticles.Count, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles by tag from S3: {Tag}", tag);
                throw new S3ArticleServiceException("GetByTag", $"Failed to get articles by tag '{tag}': {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting published articles from S3 with pagination: Page {PageNumber}, Size {PageSize}", 
                    parameters.PageNumber, parameters.PageSize);
                
                var allArticles = await GetAllArticlesFromS3Async();
                var publishedArticles = allArticles.Where(a => a.IsPublished).ToList();
                
                return CreatePaginationResult(publishedArticles, publishedArticles.Count, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published articles from S3");
                throw new S3ArticleServiceException("GetPublished", $"Failed to get published articles: {ex.Message}");
            }
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            try
            {
                if (article == null)
                {
                    throw new ArgumentNullException(nameof(article));
                }

                ValidateArticle(article);

                _logger.LogInformation("Creating new article in S3: {Title}", article.Title);
                
                var allArticles = await GetAllArticlesFromS3Async();
                
                // Check for duplicate title
                if (allArticles.Any(a => a.Title.Equals(article.Title, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new DuplicateEntityException("Article", "title", article.Title);
                }

                // Set creation metadata
                article.Id = allArticles.Any() ? allArticles.Max(a => a.Id) + 1 : 1;
                article.CreatedAt = DateTime.UtcNow;
                article.ViewCount = 0;

                // Add to collection
                allArticles.Add(article);

                // Save back to S3
                await SaveArticlesToS3Async(allArticles);
                
                // Clear cache
                _cache.Remove(S3_ARTICLES_CACHE_KEY);
                
                _logger.LogInformation("Article created successfully in S3 with ID: {ArticleId}", article.Id);
                return article;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (DuplicateEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article in S3: {Title}", article?.Title);
                throw new S3ArticleServiceException("Create", $"Failed to create article: {ex.Message}");
            }
        }

        public async Task<Article> UpdateArticleAsync(Article article)
        {
            try
            {
                if (article == null)
                {
                    throw new ArgumentNullException(nameof(article));
                }

                ValidateArticle(article);

                _logger.LogInformation("Updating article in S3: {ArticleId}", article.Id);
                
                var allArticles = await GetAllArticlesFromS3Async();
                var existingArticle = allArticles.FirstOrDefault(a => a.Id == article.Id);
                
                if (existingArticle == null)
                {
                    throw new ArticleNotFoundException(article.Id);
                }

                // Check for title duplication if changed
                if (article.Title != existingArticle.Title && 
                    allArticles.Any(a => a.Id != article.Id && a.Title.Equals(article.Title, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new DuplicateEntityException("Article", "title", article.Title);
                }

                // Preserve creation metadata and update modification time
                article.CreatedAt = existingArticle.CreatedAt;
                article.ViewCount = existingArticle.ViewCount;
                article.UpdatedAt = DateTime.UtcNow;

                // Replace in collection
                var index = allArticles.FindIndex(a => a.Id == article.Id);
                allArticles[index] = article;

                // Save back to S3
                await SaveArticlesToS3Async(allArticles);
                
                // Clear cache
                _cache.Remove(S3_ARTICLES_CACHE_KEY);
                
                _logger.LogInformation("Article updated successfully in S3: {ArticleId}", article.Id);
                return article;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (ArticleNotFoundException)
            {
                throw;
            }
            catch (DuplicateEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article in S3: {ArticleId}", article?.Id);
                throw new S3ArticleServiceException("Update", $"Failed to update article: {ex.Message}");
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting article from S3: {ArticleId}", id);
                
                var allArticles = await GetAllArticlesFromS3Async();
                var existingArticle = allArticles.FirstOrDefault(a => a.Id == id);
                
                if (existingArticle == null)
                {
                    throw new ArticleNotFoundException(id);
                }

                // Remove from collection
                allArticles.RemoveAll(a => a.Id == id);

                // Save back to S3
                await SaveArticlesToS3Async(allArticles);
                
                // Clear cache
                _cache.Remove(S3_ARTICLES_CACHE_KEY);
                
                _logger.LogInformation("Article deleted successfully from S3: {ArticleId}", id);
                return true;
            }
            catch (ArticleNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article from S3: {ArticleId}", id);
                throw new S3ArticleServiceException("Delete", $"Failed to delete article with ID {id}: {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetArticlesByNewspaperAsync(int newspaperId, PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting articles by newspaper from S3: {NewspaperId}, Page {PageNumber}, Size {PageSize}", 
                    newspaperId, parameters.PageNumber, parameters.PageSize);
                
                var allArticles = await GetAllArticlesFromS3Async();
                var filteredArticles = allArticles.Where(a => a.NewspaperId == newspaperId).ToList();
                
                return CreatePaginationResult(filteredArticles, filteredArticles.Count, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles by newspaper from S3: {NewspaperId}", newspaperId);
                throw new S3ArticleServiceException("GetByNewspaper", $"Failed to get articles by newspaper {newspaperId}: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private async Task<List<Article>> GetAllArticlesFromS3Async()
        {
            if (_cache.TryGetValue(S3_ARTICLES_CACHE_KEY, out List<Article>? cachedArticles))
            {
                return cachedArticles ?? new List<Article>();
            }

            try
            {
                var json = await _s3FileProvider.GetFileContentAsStringAsync(_articlesKey);
                var articleDtos = JsonConvert.DeserializeObject<List<ArticleDto>>(json);
                var articles = articleDtos?.Select(dto => _mapper.Map<Article>(dto)).ToList() ?? new List<Article>();

                _cache.Set(S3_ARTICLES_CACHE_KEY, articles, _cacheExpiration);
                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading articles from S3");
                throw new S3ArticleServiceException("ReadFromS3", $"Failed to read articles from S3: {ex.Message}");
            }
        }

        private async Task SaveArticlesToS3Async(List<Article> articles)
        {
            try
            {
                var articleDtos = articles.Select(a => _mapper.Map<ArticleDto>(a)).ToList();
                var json = JsonConvert.SerializeObject(articleDtos, Formatting.Indented);
                
                await _s3FileProvider.UpdateObjectAsync(_articlesKey, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving articles to S3");
                throw new S3ArticleServiceException("SaveToS3", $"Failed to save articles to S3: {ex.Message}");
            }
        }

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

        private void ValidatePaginationParameters(PaginationParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.PageNumber < 1)
            {
                parameters.PageNumber = 1;
            }

            if (parameters.PageSize < 1)
            {
                parameters.PageSize = 10;
            }

            if (parameters.PageSize > 50)
            {
                parameters.PageSize = 50;
            }
        }

        private void ValidateArticle(Article article)
        {
            if (string.IsNullOrWhiteSpace(article.Title))
            {
                throw new ArgumentException("Article title is required", nameof(article));
            }

            if (string.IsNullOrWhiteSpace(article.Content))
            {
                throw new ArgumentException("Article content is required", nameof(article));
            }

            if (article.NewspaperId <= 0)
            {
                throw new ArgumentException("Valid newspaper ID is required", nameof(article));
            }
        }

        #endregion
    }

    /// <summary>
    /// Exception specific to S3 Article Service operations
    /// </summary>
    public class S3ArticleServiceException : Exception
    {
        public string Operation { get; }

        public S3ArticleServiceException(string operation, string message) : base(message)
        {
            Operation = operation;
        }

        public S3ArticleServiceException(string operation, string message, Exception innerException) 
            : base(message, innerException)
        {
            Operation = operation;
        }
    }
}