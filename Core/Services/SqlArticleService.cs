using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Core.Domain.Exceptions;

namespace Core.Services
{
    /// <summary>
    /// SQL Server implementation of IArticleService
    /// Handles all article operations using SQL Server database through Entity Framework
    /// </summary>
    public class SqlArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SqlArticleService> _logger;

        public SqlArticleService(
            IArticleRepository articleRepository,
            IMapper mapper,
            ILogger<SqlArticleService> logger)
        {
            _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting article by ID: {ArticleId}", id);
                var article = await _articleRepository.GetByIdAsync(id);
                
                if (article == null)
                {
                    _logger.LogWarning("Article with ID {ArticleId} not found", id);
                }
                
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article by ID: {ArticleId}", id);
                throw new SqlArticleServiceException("GetById", $"Failed to get article by ID {id}: {ex.Message}");
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

                _logger.LogInformation("Getting article by title: {Title}", title);
                var article = await _articleRepository.GetByTitleAsync(title);
                
                if (article == null)
                {
                    _logger.LogWarning("Article with title '{Title}' not found", title);
                }
                
                return article;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article by title: {Title}", title);
                throw new SqlArticleServiceException("GetByTitle", $"Failed to get article by title '{title}': {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting all articles with pagination: Page {PageNumber}, Size {PageSize}", 
                    parameters.PageNumber, parameters.PageSize);
                
                return await _articleRepository.GetAllAsync(parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all articles");
                throw new SqlArticleServiceException("GetAll", $"Failed to get all articles: {ex.Message}");
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
                
                _logger.LogInformation("Getting articles by tag: {Tag}, Page {PageNumber}, Size {PageSize}", 
                    tag, parameters.PageNumber, parameters.PageSize);
                
                return await _articleRepository.GetByTagAsync(tag, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles by tag: {Tag}", tag);
                throw new SqlArticleServiceException("GetByTag", $"Failed to get articles by tag '{tag}': {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting published articles with pagination: Page {PageNumber}, Size {PageSize}", 
                    parameters.PageNumber, parameters.PageSize);
                
                return await _articleRepository.GetPublishedAsync(parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published articles");
                throw new SqlArticleServiceException("GetPublished", $"Failed to get published articles: {ex.Message}");
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

                // Check for duplicate title
                if (await _articleRepository.ExistsByTitleAsync(article.Title))
                {
                    throw new DuplicateEntityException("Article", "title", article.Title);
                }

                // Set creation metadata
                article.CreatedAt = DateTime.UtcNow;
                article.ViewCount = 0;

                _logger.LogInformation("Creating new article: {Title}", article.Title);
                var createdArticle = await _articleRepository.AddAsync(article);
                
                _logger.LogInformation("Article created successfully with ID: {ArticleId}", createdArticle.Id);
                return createdArticle;
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
                _logger.LogError(ex, "Error creating article: {Title}", article?.Title);
                throw new SqlArticleServiceException("Create", $"Failed to create article: {ex.Message}");
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

                // Check if article exists
                var existingArticle = await _articleRepository.GetByIdAsync(article.Id);
                if (existingArticle == null)
                {
                    throw new ArticleNotFoundException(article.Id);
                }

                // Check for title duplication if changed
                if (article.Title != existingArticle.Title && 
                    await _articleRepository.ExistsByTitleAsync(article.Title))
                {
                    throw new DuplicateEntityException("Article", "title", article.Title);
                }

                // Preserve creation metadata and update modification time
                article.CreatedAt = existingArticle.CreatedAt;
                article.ViewCount = existingArticle.ViewCount;
                article.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updating article: {ArticleId}", article.Id);
                var updatedArticle = await _articleRepository.UpdateAsync(article);
                
                _logger.LogInformation("Article updated successfully: {ArticleId}", article.Id);
                return updatedArticle;
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
                _logger.LogError(ex, "Error updating article: {ArticleId}", article?.Id);
                throw new SqlArticleServiceException("Update", $"Failed to update article: {ex.Message}");
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting article: {ArticleId}", id);
                
                // Check if article exists
                if (!await _articleRepository.ExistsAsync(id))
                {
                    throw new ArticleNotFoundException(id);
                }

                var result = await _articleRepository.DeleteAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Article deleted successfully: {ArticleId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete article: {ArticleId}", id);
                }
                
                return result;
            }
            catch (ArticleNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article: {ArticleId}", id);
                throw new SqlArticleServiceException("Delete", $"Failed to delete article with ID {id}: {ex.Message}");
            }
        }

        public async Task<PaginationResult<Article>> GetArticlesByNewspaperAsync(int newspaperId, PaginationParameters parameters)
        {
            try
            {
                ValidatePaginationParameters(parameters);
                
                _logger.LogInformation("Getting articles by newspaper: {NewspaperId}, Page {PageNumber}, Size {PageSize}", 
                    newspaperId, parameters.PageNumber, parameters.PageSize);
                
                return await _articleRepository.GetByNewspaperAsync(newspaperId, parameters);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles by newspaper: {NewspaperId}", newspaperId);
                throw new SqlArticleServiceException("GetByNewspaper", $"Failed to get articles by newspaper {newspaperId}: {ex.Message}");
            }
        }

        #region Private Helper Methods

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
    /// Exception specific to SQL Article Service operations
    /// </summary>
    public class SqlArticleServiceException : Exception
    {
        public string Operation { get; }

        public SqlArticleServiceException(string operation, string message) : base(message)
        {
            Operation = operation;
        }

        public SqlArticleServiceException(string operation, string message, Exception innerException) 
            : base(message, innerException)
        {
            Operation = operation;
        }
    }
}