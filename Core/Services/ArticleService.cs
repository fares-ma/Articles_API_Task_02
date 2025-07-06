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
    }
} 