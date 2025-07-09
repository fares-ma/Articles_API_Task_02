using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface IArticleService
    {
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleByTitleAsync(string title);
        Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters);
        Task<PaginationResult<Article>> GetArticlesByTagAsync(string tag, PaginationParameters parameters);
        Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters);
        Task<Article> CreateArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
        Task<PaginationResult<Article>> GetArticlesByNewspaperAsync(int newspaperId, PaginationParameters parameters);
    }
} 