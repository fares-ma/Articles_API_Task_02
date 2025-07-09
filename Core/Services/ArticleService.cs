using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;

namespace Core.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;

        public ArticleService(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _articleRepository.GetByIdAsync(id);
        }

        public async Task<Article?> GetArticleByTitleAsync(string title)
        {
            return await _articleRepository.GetByTitleAsync(title);
        }

        public async Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters)
        {
            var allArticles = await _articleRepository.GetAllAsync();
            var totalCount = await _articleRepository.GetTotalCountAsync();

            var pagedArticles = allArticles
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

        public async Task<PaginationResult<Article>> GetArticlesByTagAsync(string tag, PaginationParameters parameters)
        {
            var articlesByTag = await _articleRepository.GetByTagAsync(tag);
            var totalCount = await _articleRepository.GetByTagCountAsync(tag);

            var pagedArticles = articlesByTag
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

        public async Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters)
        {
            var publishedArticles = await _articleRepository.GetPublishedAsync();
            var totalCount = await _articleRepository.GetPublishedCountAsync();

            var pagedArticles = publishedArticles
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
            var articlesByNewspaper = await _articleRepository.GetByNewspaperAsync(newspaperId);
            var totalCount = await _articleRepository.GetByNewspaperCountAsync(newspaperId);

            var pagedArticles = articlesByNewspaper
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
    }
} 